import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {ReactiveFormsModule} from '@angular/forms';
import {RouterModule} from '@angular/router';
import {HttpClientModule} from '@angular/common/http';
import {SpinnerComponent} from './spinner/spinner.component';
import { BoolComponent } from './bool/bool.component';
import {NgbCollapseModule, NgbPaginationModule, NgbRatingModule} from '@ng-bootstrap/ng-bootstrap';

const EXPORTED_MODULES = [
  CommonModule,
  RouterModule,
  ReactiveFormsModule,
  HttpClientModule,
  NgbCollapseModule,
  NgbPaginationModule,
  NgbRatingModule,
];

const COMPONENTS = [SpinnerComponent, BoolComponent];

const DIRECTIVES = [];

const PIPES = [];

@NgModule({
  imports: [...EXPORTED_MODULES],
  exports: [...EXPORTED_MODULES, ...COMPONENTS, ...DIRECTIVES, ...PIPES],
  declarations: [...COMPONENTS, ...DIRECTIVES, ...PIPES]
})
export class SharedModule {}
