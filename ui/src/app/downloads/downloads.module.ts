import { NgModule } from '@angular/core';

import { DownloadsRoutingModule } from './downloads-routing.module';
import { ListComponent } from './list/list.component';
import { SharedModule } from '../shared/shared.module';
import { NgbProgressbarModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [ListComponent],
  imports: [SharedModule, DownloadsRoutingModule, NgbProgressbarModule],
})
export class DownloadsModule {}
