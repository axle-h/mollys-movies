import { NgModule, Optional, SkipSelf } from '@angular/core';
import { ApiModule, Configuration } from '../api';
import { SharedModule } from '../shared/shared.module';
import { LayoutComponent } from './layout/layout.component';
import { environment } from '../../environments/environment';

const EXPORTED_COMPONENTS = [LayoutComponent];

@NgModule({
  declarations: [...EXPORTED_COMPONENTS],
  imports: [
    SharedModule,
    ApiModule.forRoot(() => new Configuration({ basePath: environment.basePath })),
  ],
  exports: [...EXPORTED_COMPONENTS],
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error(
        'CoreModule has already been loaded. Import Core modules in the AppModule only.',
      );
    }
  }
}
