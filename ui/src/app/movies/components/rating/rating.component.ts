import {Component, Input} from '@angular/core';
import {Movie} from '../../../api';

@Component({
  selector: 'mm-rating',
  template: '<i *ngFor="let i of [1, 2, 3, 4, 5]" [ngClass]="getStar(i)"></i>'
})
export class RatingComponent {
  @Input() movie: Movie;

  getStar(i: number) {
    const rating = this.movie.rating / 2;
    if (rating >= i - 0.3) {
      return 'fas fa-star';
    }

    if (rating >= i - 0.6) {
      return 'fas fa-star-half-alt';
    }

    return 'far fa-star';
  }

}
