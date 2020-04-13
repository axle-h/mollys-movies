import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {LayoutComponent} from './core/layout/layout.component';

const routes: Routes = [
  {
    path: 'movies', component: LayoutComponent,
    loadChildren() {
      return import('./movies/movies.module').then(m => m.MoviesModule);
    },
  },
  {
    path: 'downloads', component: LayoutComponent,
    loadChildren() {
      return import('./downloads/downloads.module').then(m => m.DownloadsModule);
    },
  },
  {
    path: 'scraper', component: LayoutComponent,
    loadChildren() {
      return import('./scraper/scraper.module').then(m => m.ScraperModule);
    },
  },
  { path: '**', redirectTo: '/movies', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { onSameUrlNavigation: 'reload' })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
