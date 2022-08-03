import { Component, Input } from '@angular/core';
import { Movie } from '../../../api';

@Component({
  selector: 'mm-badges',
  templateUrl: './badges.component.html',
})
export class BadgesComponent {
  @Input() movie: Movie;

  get qualities() {
    const qualities = this.movie.torrents.map(x => x.quality).sort((x, y) => x.localeCompare(y));
    return new Set(qualities);
  }

  get types() {
    const types = this.movie.torrents.map(x => x.type).sort((x, y) => x.localeCompare(y));
    return new Set(types);
  }
}
