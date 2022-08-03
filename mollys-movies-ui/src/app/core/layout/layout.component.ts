import { Component } from '@angular/core';

@Component({
  selector: 'mm-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent {
  public isMenuCollapsed = true;

  navClick() {
    this.isMenuCollapsed = true;
  }
}
