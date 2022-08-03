import { Component, OnInit } from '@angular/core';
import { Scrape, ScrapeService } from '../../api';
import { flatMap } from 'rxjs/operators';

@Component({
  selector: 'mm-list',
  templateUrl: './list.component.html',
})
export class ListComponent implements OnInit {
  scrapes: Scrape[] | null = null;

  constructor(private readonly scraperService: ScrapeService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.scrapes = null;
    this.scraperService.getAllScrapes().subscribe(value => (this.scrapes = value));
  }

  createScrape() {
    this.scraperService
      .scrape()
      .pipe(flatMap(() => this.scraperService.getAllScrapes()))
      .subscribe(value => (this.scrapes = value));
  }
}
