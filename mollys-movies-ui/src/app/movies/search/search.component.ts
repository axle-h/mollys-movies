import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  GenreService,
  Movie,
  MoviePaginatedData,
  MoviesOrderBy,
  MoviesService,
  SearchMoviesRequestParams,
} from '../../api';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { debounce, filter, flatMap, map, mergeAll, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, from, interval, Observable, of, Subject, Subscription } from 'rxjs';
import { CollectionViewer, DataSource } from '@angular/cdk/collections';

type SearchState = Omit<SearchMoviesRequestParams, 'limit'>;

const LIMIT = 20;

const defaultState: SearchState = Object.freeze({
  page: 1,
  title: '',
  genre: '',
  downloaded: true,
  yearFrom: null,
  yearTo: null,
  ratingFrom: null,
  ratingTo: null,
  orderBy: MoviesOrderBy.Title,
  orderByDescending: false,
});

function unsetDefaults(state: SearchState) {
  // unset defaults, no point these extending url
  for (const [key, value] of Object.entries(state)) {
    if (value === defaultState[key]) {
      state[key] = undefined;
    }
  }
  return state;
}

export class MovieDataSource extends DataSource<Movie> {
  private readonly pages: Movie[][] = [];

  constructor(
    private readonly getPage: (page: number) => Observable<MoviePaginatedData>,
    private readonly $count: Subject<number>,
    private readonly $page: BehaviorSubject<number>,
  ) {
    super();
  }

  connect(collectionViewer: CollectionViewer): Observable<Movie[]> {
    const $page1 = of(1);
    const $scroll = collectionViewer.viewChange.pipe(
      flatMap(range => {
        const [minPage, maxPage] = [range.start, range.end].map(x => Math.floor(x / LIMIT) + 1);
        if (this.$page.value !== minPage) {
          this.$page.next(minPage);
        }
        return [...Array(maxPage - minPage + 1).keys()].map(i => i + minPage);
      }),
      filter(page => !this.pages[page - 1]),
    );

    return from([$page1, $scroll]).pipe(
      mergeAll(),
      switchMap(page =>
        this.getPage(page).pipe(
          map(result => {
            this.$count.next(result.count);
            this.pages[page - 1] = result.data;
            return Object.values(this.pages).reduce((x, y) => [...x, ...y]);
          }),
        ),
      ),
    );
  }

  disconnect(collectionViewer: CollectionViewer): void {}
}

@Component({
  selector: 'mm-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
})
export class SearchComponent implements OnInit, OnDestroy {
  private readonly subs: Subscription[] = [];
  readonly $availableGenres = this.genreService.getAllGenres();
  searchForm: UntypedFormGroup;
  filtersCollapsed = true;
  sortCollapsed = true;
  dataSource: MovieDataSource;

  constructor(
    private readonly moviesService: MoviesService,
    private readonly genreService: GenreService,
    private readonly activatedRoute: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: UntypedFormBuilder,
  ) {}

  $count = new BehaviorSubject<number>(0);

  get orderByKeys() {
    return Object.keys(MoviesOrderBy);
  }

  get maxYear() {
    return new Date().getFullYear();
  }

  ngOnInit(): void {
    this.searchForm = this.fb.group({
      title: [defaultState.title],
      downloaded: [defaultState.downloaded],
      genre: [defaultState.genre],
      yearFrom: [defaultState.yearFrom, [Validators.min(1900), Validators.max(this.maxYear)]],
      yearTo: [defaultState.yearTo, [Validators.min(1900), Validators.max(this.maxYear)]],
      ratingFrom: [defaultState.ratingFrom, [Validators.min(0), Validators.max(5)]],
      ratingTo: [defaultState.ratingTo, [Validators.min(0), Validators.max(5)]],
      orderBy: [defaultState.orderBy],
      orderByDescending: [defaultState.orderByDescending],
    });

    this.subs.push(
      from([
        // only title is debounced
        this.searchForm.get('title').valueChanges.pipe(debounce(() => interval(500))),
        ...Object.entries(this.searchForm.controls)
          .filter(([k]) => k !== 'title')
          .map(([, v]) => v.valueChanges),
      ])
        .pipe(mergeAll())
        // reset the page back to number 1, this will trigger the query string subscription to reset the data source
        .subscribe(() => $page.next(1)),
    );

    const $page = new BehaviorSubject<number>(1);
    this.subs.push(
      $page.subscribe(page =>
        this.router.navigate([], {
          relativeTo: this.activatedRoute,
          queryParams: unsetDefaults({ ...this.formState, page }),
        }),
      ),
    );

    this.subs.push(
      this.activatedRoute.queryParamMap
        .pipe(
          filter(() => !this.dataSource || !this.searchForm.pristine),
          map<ParamMap, SearchState>(query => ({
            page: parseInt(query.get('page'), 10) || defaultState.page,
            title: query.get('title') || defaultState.title,
            downloaded: (query.get('downloaded') || defaultState.downloaded.toString()) === 'true',
            genre: query.get('genre') || defaultState.genre,
            yearFrom: parseInt(query.get('yearFrom'), 10) || defaultState.yearFrom,
            yearTo: parseInt(query.get('yearTo'), 10) || defaultState.yearTo,
            ratingFrom: parseInt(query.get('ratingFrom'), 10) || defaultState.ratingFrom,
            ratingTo: parseInt(query.get('ratingTo'), 10) || defaultState.ratingTo,
            orderBy: (query.get('orderBy') as MoviesOrderBy) || defaultState.orderBy,
            orderByDescending:
              (query.get('orderByDescending') || defaultState.orderByDescending.toString()) ===
              'true',
          })),
        )
        .subscribe(x => {
          if (!this.dataSource) {
            this.reset(x);
            // TODO somehow scroll to initial page
          } else {
            this.searchForm.markAsPristine();
          }

          const state = this.formState;
          this.dataSource = new MovieDataSource(
            page =>
              this.moviesService.searchMovies({
                ...state,
                page,
                limit: LIMIT,
                downloaded: state.downloaded ? undefined : false,
                ratingFrom: state.ratingFrom && state.ratingFrom * 2,
                ratingTo: state.ratingTo && state.ratingTo * 2,
              }),
            this.$count,
            $page,
          );
        }),
    );
  }

  get isDirty() {
    return Object.entries(this.formState).some(([k, v]) => defaultState[k] !== v);
  }

  reset(state: SearchState = defaultState) {
    this.searchForm.reset(state);
    this.dataSource = null;
  }

  toggleFilters() {
    this.filtersCollapsed = !this.filtersCollapsed;
    this.sortCollapsed = true;
  }

  toggleSort() {
    this.sortCollapsed = !this.sortCollapsed;
    this.filtersCollapsed = true;
  }

  select(movie: Movie) {
    return this.router.navigate([movie.imdbCode], { relativeTo: this.activatedRoute });
  }

  ngOnDestroy(): void {
    for (const sub of this.subs) {
      sub.unsubscribe();
    }
  }

  submit() {}

  private get formState(): SearchState {
    return Object.entries(this.searchForm.controls)
      .map(([k, v]) => ({ [k]: v.value }))
      .reduce((x, y) => ({ ...x, ...y }));
  }
}
