import { cloneDeep, merge, mergeWith } from 'lodash';
import { DeepPartial } from './stubs.types';

export type FakeFn<Faked, Options = any> = (
  partialOrPartials?: DeepPartial<Faked> | DeepPartial<Faked>[],
  options?: Options,
) => Faked;

/**
 * The lodash merge function is too safe for merging partials, where we want all properties,
 * including arrays to be strictly overridden.
 */
function mergePartials<T>(...items: T[]): T {
  const truthyItems = items.filter(x => x);
  switch (truthyItems.length) {
    case 0:
      return {} as T;
    case 1:
      return truthyItems[0];
    default:
      return mergeWith({} as T, ...truthyItems, (x: any, y: any) => {
        if (Array.isArray(x)) {
          return y || x;
        }
      });
  }
}

function unwrapPartial<Faked>(
  partialOrPartials?: DeepPartial<Faked> | DeepPartial<Faked>[],
): DeepPartial<Faked> {
  return Array.isArray(partialOrPartials)
    ? mergePartials(...partialOrPartials)
    : partialOrPartials
    ? cloneDeep(partialOrPartials)
    : {};
}

export function fake<Faked, Options = any>(
  factory: (options: Options, partial: DeepPartial<Faked>) => Faked,
): FakeFn<Faked, Options> {
  return (partialOrPartials, options) => {
    const partial = unwrapPartial(partialOrPartials);
    const fake = factory(options || ({} as any), partial);
    return merge(fake, partial as Faked);
  };
}
