import { NgModule } from '@angular/core';

import { ScraperRoutingModule } from './scraper-routing.module';
import { ListComponent } from './list/list.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [ListComponent],
  imports: [SharedModule, ScraperRoutingModule],
})
export class ScraperModule {}
