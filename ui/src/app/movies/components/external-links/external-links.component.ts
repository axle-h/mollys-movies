import {Component, Input} from '@angular/core';
import {Movie} from '../../../api';

@Component({
  selector: 'mm-external-links',
  templateUrl: './external-links.component.html',
  styleUrls: ['./external-links.component.scss']
})
export class ExternalLinksComponent {
  @Input() movie: Movie;
}
