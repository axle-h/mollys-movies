import { Component, Input } from '@angular/core';

@Component({
  selector: 'mm-bool',
  template: `
    <i class="fas fa-check text-success" *ngIf="value; else minus"></i>
    <ng-template #minus>
      <i class="fas fa-minus text-danger"></i>
    </ng-template>
  `,
})
export class BoolComponent {
  @Input() value: boolean;
}
