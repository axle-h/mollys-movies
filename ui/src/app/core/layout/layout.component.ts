import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'mm-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {
  public isMenuCollapsed = true;

  constructor() { }

  ngOnInit(): void {
  }

  navClick() {
    this.isMenuCollapsed = true;
  }
}
