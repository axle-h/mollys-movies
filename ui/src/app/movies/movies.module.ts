import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MoviesRoutingModule } from './movies-routing.module';
import { SearchComponent } from './search/search.component';
import { ViewComponent } from './view/view.component';
import { SharedModule } from '../shared/shared.module';
import { RatingComponent } from './components/rating/rating.component';
import { ExternalLinksComponent } from './components/external-links/external-links.component';
import { PosterComponent } from './components/poster/poster.component';
import { BadgesComponent } from './components/badges/badges.component';

@NgModule({
  declarations: [
    SearchComponent,
    ViewComponent,
    RatingComponent,
    ExternalLinksComponent,
    PosterComponent,
    BadgesComponent,
  ],
  imports: [CommonModule, MoviesRoutingModule, SharedModule],
})
export class MoviesModule {}
