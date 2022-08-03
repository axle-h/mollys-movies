import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'mm-spinner',
  template: ` <div class="spinner" *ngIf="display">
    <i class="fas fa-sync fa-spin fa-3x"></i>
  </div>`,
  styleUrls: ['./spinner.component.scss'],
})
export class SpinnerComponent implements OnInit {
  display = false;
  ngOnInit(): void {
    setTimeout(() => (this.display = true), 150);
  }
}
