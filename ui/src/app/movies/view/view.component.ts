import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { map, mergeMap } from 'rxjs/operators';
import { Movie, MoviesService, MovieTorrentService, Torrent } from '../../api';
import { throwError } from 'rxjs';

const typePref = {
  bluray: 1,
  web: 2,
};

const qualityPref = {
  '720p': 1,
  '1080p': 2,
};

function getTypePref(torrent: Torrent) {
  return typePref[torrent.type] || Object.keys(typePref).length + 1;
}

function getQualityPref(torrent: Torrent) {
  return qualityPref[torrent.quality] || Object.keys(qualityPref).length + 1;
}

@Component({
  selector: 'mm-view',
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.scss'],
})
export class ViewComponent implements OnInit {
  movie: Movie | null = null;

  constructor(
    private readonly location: Location,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly moviesService: MoviesService,
    private readonly movieTorrentService: MovieTorrentService,
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(
        map(p => parseInt(p.id, 10)),
        mergeMap(id =>
          id
            ? this.moviesService.getMovie({ id })
            : throwError(new Error('No id parameter provided')),
        ),
      )
      .subscribe(x => (this.movie = x));
  }

  back() {
    this.location.back();
  }

  pickTorrent() {
    return this.movie.movieSources
      .map(x => x.torrents)
      .reduce((agg, x) => [...agg, ...x])
      .sort((x, y) => {
        if (x.type === y.type && x.quality === y.quality) {
          return 0;
        }

        const xq = getQualityPref(x);
        const yq = getQualityPref(y);

        if (xq !== yq) {
          return xq - yq;
        }

        return getTypePref(x) - getTypePref(y);
      })[0];
  }

  download() {
    const torrent = this.pickTorrent();
    this.movieTorrentService
      .downloadMovie({ movieId: this.movie.id, torrentId: torrent.id })
      .subscribe(() => {
        return this.router.navigate(['/downloads']);
      });
  }
}
