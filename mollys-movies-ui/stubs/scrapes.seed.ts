import { DeepPartial, MaybeReadonlyArray } from './stubs.types';
import { Scrape } from './api/model';
import { SeedFn, seedModule, SeedModule } from './seed.types';
import { fakeScrape } from './mollys-movies-api.fake';

export const FAKE_SCRAPES: ReadonlyArray<DeepPartial<Scrape>> = Object.freeze([
  {},
  { success: true },
  { success: false },
]);

export function scrape(partial: DeepPartial<Scrape>): SeedFn {
  return context => {
    const scrape = fakeScrape(partial);
    context.client.apiv1.get('/scrape').collectList(scrape);
  };
}

export interface ScrapeSeedOptions {
  scrapes?: MaybeReadonlyArray<DeepPartial<Scrape>>;
}

export function scrapes({
  scrapes: fakeScrapes = FAKE_SCRAPES,
}: ScrapeSeedOptions = {}): SeedModule {
  return seedModule({ title: 'scrapes' }, ...fakeScrapes.map(x => scrape(x)));
}
