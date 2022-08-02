import { Pipe, PipeTransform } from '@angular/core';
import { DecimalPipe } from '@angular/common';

@Pipe({
  name: 'quantity',
})
export class QuantityPipe extends DecimalPipe implements PipeTransform {
  transform(value: number | string | null | undefined, singular: string): any {
    const valueString = super.transform(value);
    return value === 1 ? `${valueString} ${singular}` : `${valueString} ${singular}s`;
  }
}
