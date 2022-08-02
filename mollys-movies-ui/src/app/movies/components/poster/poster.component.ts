import { Component, Input } from '@angular/core';
import { Movie } from '../../../api';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'mm-poster',
  templateUrl: './poster.component.html',
  styleUrls: ['./poster.component.scss'],
})
export class PosterComponent {
  @Input() movie: Movie;
  @Input() thumb: boolean;
  basePath = environment.basePath;
}
