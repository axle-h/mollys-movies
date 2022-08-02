import { SeedFn, seedModule, SeedModule } from './seed.types';
import { fakeLiveTransmissionStatus, fakeMovieDownload } from './mollys-movies-api.fake';
import { DeepPartial, MaybeReadonlyArray } from './stubs.types';
import { MovieDownload, MovieDownloadStatusCode } from './api/model';

export const FAKE_DOWNLOADS: ReadonlyArray<DeepPartial<MovieDownload>> = Object.freeze([
  { status: MovieDownloadStatusCode.Started },
  { status: MovieDownloadStatusCode.Started },
  { status: MovieDownloadStatusCode.Started },
  { status: MovieDownloadStatusCode.Started },
  { status: MovieDownloadStatusCode.Downloaded },
  { status: MovieDownloadStatusCode.Complete },
]);

export function download(partial: DeepPartial<MovieDownload>): SeedFn {
  return context => {
    const download = fakeMovieDownload(partial);
    context.client.apiv1.get(`/transmission/${download.externalId}`).ok(download);

    if (download.status == MovieDownloadStatusCode.Started) {
      const status = fakeLiveTransmissionStatus();
      context.client.apiv1.get(`/movies/${download.imdbCode}/torrents`).ok(status);
    }

    context.client.apiv1.get('/transmission').collectPaginated(download);
  };
}

export interface TransmissionSeedOptions {
  downloads?: MaybeReadonlyArray<DeepPartial<MovieDownload>>;
}

export function transmission({
  downloads = FAKE_DOWNLOADS,
}: TransmissionSeedOptions = {}): SeedModule {
  return seedModule({ title: 'transmission' }, ...downloads.map(x => download(x)));
}
