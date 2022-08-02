import { fake } from './fake';
import {
  LiveTransmissionStatus,
  LocalMovieSource,
  Movie,
  MovieDownload,
  MovieDownloadStatusCode,
  Scrape,
  ScraperType,
  ScrapeSource,
  Torrent,
} from './api/model';
import { faker } from '@faker-js/faker';

export function imdbCode(): string {
  return `tt${faker.random.numeric(7)}`;
}

export function movieTitle(): string {
  return faker.helpers.arrayElement([
    'Back to the Future',
    'Desperado',
    'Night at the Museum',
    'Robocop',
    'Ghostbusters',
    'Cool World',
    'Donnie Darko',
    'Double Indemnity',
    'The Spanish Prisoner',
    'The Smurfs',
    'Dead Alive',
    'Army of Darkness',
    'Peter Pan',
    'The Jungle Story',
    'Red Planet',
    'Deep Impact',
    'The Long Kiss Goodnight',
    'Juno',
    '(500) Days of Summer',
    'The Dark Knight',
    'Bringing Down the House',
    'Se7en',
    'Chocolat',
    'The American',
    'The American President',
    'Hudsucker Proxy',
    'Conan the Barbarian',
    'Shrek',
    'The Fox and the Hound',
    'Lock, Stock, and Two Barrels',
    'Date Night',
    '200 Cigarettes',
    '9 1/2 Weeks',
    'Iron Man 2',
    'Tombstone',
    'Young Guns',
    'Fight Club',
    'The Cell',
    'The Unborn',
    'Black Christmas',
    'The Change-Up',
    'The Last of the Mohicans',
    'Shutter Island',
    'Ronin',
    'Ocean’s 11',
    'Philadelphia',
    'Chariots of Fire',
    'M*A*S*H',
    'Walking and Talking',
    'Walking Tall',
    'The 40 Year Old Virgin',
    'Superman III',
    'The Hour',
    'The Slums of Beverly Hills',
    'Secretary',
    'Secretariat',
    'Pretty Woman',
    'Sleepless in Seattle',
    'The Iron Mask',
    'Smoke',
    'Schindler’s List',
    'The Beverly Hillbillies',
    'The Ugly Truth',
    'Bounty Hunter',
    'Say Anything',
    '8 Seconds',
    'Metropolis',
    'Indiana Jones and the Temple of Doom',
    'Kramer vs. Kramer',
    'The Manchurian Candidate',
    'Raging Bull',
    'Heat',
    'About Schmidt',
    'Re-Animator',
    'Evolution',
    'Gone in 60 Seconds',
    'Wanted',
    'The Man with One Red Shoe',
    'The Jerk',
    'Whip It',
    'Spanking the Monkey',
    'Steel Magnolias',
    'Horton Hears a Who',
    'Honey',
    'Brazil',
    'Gorillas in the Mist',
    'Before Sunset',
    'After Dark',
    'From Dusk til Dawn',
    'Cloudy with a Chance of Meatballs',
    'Harvey',
    'Mr. Smith Goes to Washington',
    'L.A. Confidential',
    'Little Miss Sunshine',
    'The Future',
    'Howard the Duck',
    'Howard’s End',
    'The Innkeeper',
    'Revolutionary Road',
    'Interstellar',
  ]);
}

export function movieYear(): number {
  return faker.datatype.number({ min: 1980, max: new Date().getFullYear() });
}

export function movieNameWithYear(): string {
  return `${movieTitle()} (${movieYear()})`;
}

export function language(): string {
  return faker.helpers.arrayElement(['de', 'en', 'es', 'fr', 'zh']);
}

export function movieRating(): number {
  return faker.datatype.float({ min: 0, max: 10, precision: 0.1 });
}

export function youtubeVideoId(): string {
  return `${faker.random.alpha(6)}-${faker.random.alphaNumeric(4)}`;
}

export const GENRES = Object.freeze([
  'Action',
  'Comedy',
  'Drama',
  'Fantasy',
  'Horror',
  'Romance',
  'Thriller',
]);

export function genres(count: number = 2): string[] {
  return faker.helpers.arrayElements(GENRES, count);
}

export function movieSource(): string {
  return faker.helpers.arrayElement(['Yts', 'PirateBay']);
}

export enum TorrentType {
  Bluray = 'bluray',
  Dvd = 'dvd',
  Web = 'web',
}

export function torrentType(): TorrentType {
  return faker.helpers.objectValue(TorrentType);
}

export enum TorrentQuality {
  HD = '720p',
  FHD = '1080p',
  UHD = '2160p',
  _3D = '3D',
}

export function torrentQuality(): TorrentQuality {
  return faker.helpers.objectValue(TorrentQuality);
}

export function torrentHash(): string {
  return faker.random.alpha({ count: 32, casing: 'upper' });
}

export const fakeTorrent = fake<Torrent>(() => ({
  source: movieSource(),
  url: faker.internet.url(),
  type: torrentType(),
  quality: torrentQuality(),
  hash: torrentHash(),
  sizeBytes: faker.datatype.number({ min: 104900000, max: 1074000000 }),
}));

export const fakeLocalMovieSource = fake<LocalMovieSource>(() => ({
  source: 'Plex',
  dateCreated: faker.date.past().toISOString(),
  dateScraped: faker.date.past().toISOString(),
}));

export const fakeMovieDownload = fake<MovieDownload>(() => ({
  imdbCode: imdbCode(),
  name: movieNameWithYear(),
  status: faker.helpers.objectValue(MovieDownloadStatusCode),
  externalId: faker.datatype.uuid(),
}));

export const fakeLiveTransmissionStatus = fake<LiveTransmissionStatus>((options, partial) => {
  const complete = partial.complete ?? faker.datatype.boolean();
  const base = { name: movieNameWithYear(), complete };
  return complete
    ? base
    : {
        ...base,
        eta: 120,
        percentComplete: faker.datatype.float({ min: 0, max: 1, precision: 0.01 }),
        stalled: false,
      };
});

export const fakeMovie = fake<Movie>((options, partial) => {
  const imdb = partial.imdbCode || imdbCode();
  const title = partial.title || movieTitle();
  const year = partial.year || movieYear();
  return {
    imdbCode: imdb,
    title,
    language: language(),
    year,
    rating: movieRating(),
    description: faker.lorem.sentences(2),
    youTubeTrailerCode: youtubeVideoId(),
    imageFilename: `${imdb}.jpg`,
    genres: genres(),
    torrents: [
      fakeTorrent({ type: TorrentType.Bluray, quality: TorrentQuality.HD }),
      fakeTorrent({ type: TorrentType.Bluray, quality: TorrentQuality.FHD }),
    ],
    localSource: fakeLocalMovieSource(),
    download: fakeMovieDownload({
      imdbCode: imdb,
      name: `${title} (${year})`,
    }),
  };
});

export interface FakeScrapeOptions {
  complete?: boolean;
}

export const fakeScrapeSource = fake<ScrapeSource, FakeScrapeOptions>((options, partial) => {
  const complete =
    partial.success !== undefined ? true : options.complete ?? faker.datatype.boolean();
  const base = {
    source: movieSource(),
    startDate: faker.date.past().toISOString(),
    type: faker.helpers.objectValue(ScraperType),
  };
  if (complete) {
    const success = faker.datatype.boolean();
    return {
      ...base,
      endDate: faker.date.recent().toISOString(),
      success,
      movieCount: success ? faker.datatype.number() : 0,
      torrentCount: base.type === ScraperType.Torrent && success ? faker.datatype.number() : 0,
      error: success ? null : faker.hacker.phrase(),
    };
  }
  return base;
});

export const fakeScrape = fake<Scrape, FakeScrapeOptions>((options, partial) => {
  const complete = partial.success !== undefined ? true : options.complete ?? false;
  const base = {
    id: faker.datatype.uuid(),
    startDate: faker.date.past().toISOString(),
  };
  if (complete) {
    const success = faker.datatype.boolean();
    const source = fakeScrapeSource({ success });
    return {
      ...base,
      success,
      endDate: source.endDate,
      localMovieCount: source.type === ScraperType.Local ? source.movieCount : 0,
      movieCount: source.type === ScraperType.Torrent ? source.movieCount : 0,
      torrentCount: source.torrentCount,
      sources: [source],
    };
  }
  return {
    ...base,
    localMovieCount: 0,
    movieCount: 0,
    torrentCount: 0,
    sources: [fakeScrapeSource({}, { complete: false })],
  };
});
