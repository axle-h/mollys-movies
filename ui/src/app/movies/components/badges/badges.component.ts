import { Component, Input } from '@angular/core';
import { Movie } from '../../../api';

@Component({
  selector: 'mm-badges',
  templateUrl: './badges.component.html',
})
export class BadgesComponent {
  @Input() movie: Movie;

  get qualities() {
    const qualities = this.movie.movieSources.map(x => x.torrents.map(t => t.quality))
      .reduce((agg, x) => [...agg, ...x])
      .sort((x, y) => x.localeCompare(y));
    return new Set(qualities);
  }

  get types() {
    const types = this.movie.movieSources.map(x => x.torrents.map(t => t.type))
      .reduce((agg, x) => [...agg, ...x])
      .sort((x, y) => x.localeCompare(y));
    return new Set(types);
  }
}
