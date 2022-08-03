import { QuantityPipe } from './quantity.pipe';

describe('QuantityPipe', () => {
  const pipe = new QuantityPipe('en-GB');

  it('transforms zero', () => {
    const observed = pipe.transform(0, 'thing');
    expect(observed).toEqual('0 things');
  });

  it('transforms singular', () => {
    const observed = pipe.transform(1, 'thing');
    expect(observed).toEqual('1 thing');
  });

  it('transforms plural', () => {
    const observed = pipe.transform(2, 'thing');
    expect(observed).toEqual('2 things');
  });

  it('transforms many', () => {
    const observed = pipe.transform(123456789, 'thing');
    expect(observed).toEqual('123,456,789 things');
  });
});
