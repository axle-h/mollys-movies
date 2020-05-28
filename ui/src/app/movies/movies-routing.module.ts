import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {SearchComponent} from './search/search.component';
import {ViewComponent} from './view/view.component';


const routes: Routes = [
  { path: '',
    children: [
      { path: ':id', component: ViewComponent },
      { path: '', component: SearchComponent, pathMatch: 'full' },
      { path: '**', redirectTo: '', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MoviesRoutingModule { }
