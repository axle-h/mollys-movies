import { Component, OnDestroy, OnInit } from '@angular/core';
import { GenreService, Movie, MoviesOrderBy, MoviesService } from '../../api';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { debounce, map, mergeAll, switchMap, tap } from 'rxjs/operators';
import { from, interval, Subscription } from 'rxjs';

interface SearchState {
  page: number;
  limit: number;
  title?: string;
  showDownloaded: boolean;
  genre?: string;
  orderBy?: MoviesOrderBy;
  descending: boolean;
}

function filtersEqual(x: SearchState, y: SearchState) {
  return x.title === y.title
    && x.showDownloaded === y.showDownloaded
    && x.genre === y.genre
    && x.descending === y.descending
    && x.orderBy === y.orderBy;
}

const defaultState: SearchState = {
  page: 1,
  limit: 20,
  title: '',
  showDownloaded: true,
  genre: '',
  orderBy: MoviesOrderBy.Title,
  descending: false,
};

@Component({
  selector: 'mm-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
})
export class SearchComponent implements OnInit, OnDestroy {
  subs: Subscription[] = [];
  movies: Movie[] | null = null;
  count?: number;

  searchForm: FormGroup;
  routerState: SearchState;

  filtersCollapsed = true;
  sortCollapsed = true;

  availableGenres: string[] = [];

  constructor(
    private readonly moviesService: MoviesService,
    private readonly genreService: GenreService,
    private readonly activatedRoute: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder) { }

  get title() {
    return this.searchForm.get('title');
  }

  get showDownloaded() {
    return this.searchForm.get('showDownloaded');
  }

  get orderBy() {
    return this.searchForm.get('orderBy');
  }

  get descending() {
    return this.searchForm.get('descending');
  }

  get genre() {
    return this.searchForm.get('genre');
  }

  get orderByKeys() {
    return Object.keys(MoviesOrderBy);
  }

  // '' | true => true => undefined
  // 'false => false => false
  ngOnInit(): void {
    this.genreService.getAllGenres().subscribe(x => this.availableGenres = x);

    this.activatedRoute.queryParamMap.pipe(
      map<ParamMap, SearchState>(query => ({
        page: parseInt(query.get('page'), 10) || defaultState.page,
        limit: parseInt(query.get('limit'), 10) || defaultState.limit,
        title: query.get('title') || defaultState.title,
        showDownloaded: (query.get('showDownloaded') || defaultState.showDownloaded.toString()) === 'true',
        genre: query.get('genre') || defaultState.genre,
        orderBy: (query.get('orderBy') as MoviesOrderBy) || defaultState.orderBy,
        descending: (query.get('descending') || defaultState.descending.toString()) === 'true',
      })),
      tap(x => {
        this.routerState = x;
        this.searchForm = this.fb.group({
          title: [x.title],
          showDownloaded: [x.showDownloaded],
          genre: [x.genre],
          orderBy: [x.orderBy],
          descending: [x.descending],
        });
        this.subs.push(this.title.valueChanges
          .pipe(debounce(() => interval(500)))
          .subscribe(() => this.search()));

        this.subs.push(from([
          this.showDownloaded.valueChanges,
          this.genre.valueChanges,
          this.orderBy.valueChanges,
          this.descending.valueChanges
        ])
          .pipe(mergeAll())
          .subscribe(() => this.search()));
      }),
      switchMap(x => this.moviesService.searchMovies(
        x.title || undefined,
        undefined,
        undefined,
        x.showDownloaded ? undefined : false,
        x.genre || undefined,
        x.orderBy || undefined,
        x.descending || undefined,
        x.page,
        x.limit)))
      .subscribe(value => {
        this.movies = value.data;
        this.routerState.page = value.page;
        this.routerState.limit = value.limit;
        this.count = value.count;
      });
  }

  search() {
    const state = this.state;

    if (!filtersEqual(state, this.routerState)) {
      // filters changed so go back to page 1.
      state.page = 1;
    }

    // unset defaults, no point these extending url
    for (const [key, value] of Object.entries(state)) {
      if (defaultState[key] === value) {
        state[key] = undefined;
      }
    }

    return this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: state,
    });
  }

  get isDirty() {
    return !filtersEqual(this.state, defaultState);
  }

  reset() {
    this.title.setValue(defaultState.title);
    this.showDownloaded.setValue(defaultState.showDownloaded);
    this.genre.setValue(defaultState.genre);
    this.orderBy.setValue(defaultState.orderBy);
    this.descending.setValue(defaultState.descending);
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
    return this.router.navigate([movie.id], { relativeTo: this.activatedRoute });
  }

  ngOnDestroy(): void {
    for (const sub of this.subs) {
      sub.unsubscribe();
    }
  }

  submit() {}

  private get state(): SearchState {
    return {
      page: this.routerState.page,
      limit: this.routerState.limit,
      title: this.title.value,
      showDownloaded: this.showDownloaded.value,
      genre: this.genre.value,
      orderBy: this.orderBy.value,
      descending: this.descending.value,
    };
  }
}
