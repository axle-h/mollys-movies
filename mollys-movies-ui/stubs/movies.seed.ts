import { DeepPartial, MaybeReadonlyArray } from './stubs.types';
import { Movie, MoviePaginatedData } from './api/model';
import { SeedFn, seedModule, SeedModule } from './seed.types';
import { fakeMovie, GENRES as FAKE_GENRES } from './mollys-movies-api.fake';

export const FAKE_MOVIES: ReadonlyArray<DeepPartial<Movie>> = Object.freeze([
  {
    imdbCode: 'tt0816692',
    title: 'Interstellar',
    language: 'English',
    year: 2014,
    rating: 8.6,
    description:
      "Earth's future has been riddled by disasters, famines, and droughts. There is only one way to ensure mankind's survival: Interstellar travel. A newly discovered wormhole in the far reaches of our solar system allows a team of astronauts to go where no man has gone before, a planet that may have the right environment to sustain human life.",
    youTubeTrailerCode: '827FNDpQWrQ',
    genres: ['Adventure', 'Thriller', 'Sci-Fi', 'Drama', 'Action'],
    download: null,
    localSource: null,
  },
  {
    imdbCode: 'tt0110912',
    title: 'Pulp Fiction',
    language: 'English',
    year: 1994,
    rating: 8.9,
    description:
      'Jules Winnfield (Samuel L. Jackson) and Vincent Vega (John Travolta) are two hit men who are out to retrieve a suitcase stolen from their employer, mob boss Marsellus Wallace (Ving Rhames). Wallace has also asked Vincent to take his wife Mia (Uma Thurman) out a few days later when Wallace himself will be out of town. Butch Coolidge (Bruce Willis) is an aging boxer who is paid by Wallace to lose his fight. The lives of these seemingly unrelated people are woven together comprising of a series of funny, bizarre and uncalled-for incidents.',
    youTubeTrailerCode: 'tGpTpVyI_OQ',
    genres: ['Drama', 'Crime', 'Action'],
    download: null,
    localSource: null,
  },
  {
    imdbCode: 'tt0076759',
    title: 'Star Wars: Episode IV - A New Hope',
    language: 'English',
    year: 1977,
    rating: 8.6,
    description:
      'The Imperial Forces, under orders from cruel Darth Vader, hold Princess Leia hostage in their efforts to quell the rebellion against the Galactic Empire. Luke Skywalker and Han Solo, captain of the Millennium Falcon, work together with the companionable droid duo R2-D2 and C-3PO to rescue the beautiful princess, help the Rebel Alliance and restore freedom and justice to the Galaxy.',
    youTubeTrailerCode: 'vZ734NWnAHA',
    genres: ['Sci-Fi', 'Fantasy', 'Adventure', 'Action'],
  },
]);

export function movie(partial: DeepPartial<Movie>): SeedFn {
  return context => {
    const movie = fakeMovie(partial);
    context.client.apiv1.get(`/movies/${movie.imdbCode}`).ok(movie);
    for (let torrent of movie.torrents || []) {
      context.client.apiv1
        .post(`/movies/${movie.imdbCode}/torrents/quality/${torrent.quality}/type/${torrent.type}`)
        .ok();
    }

    context.client.apiv1.get('/movies').collectPaginated(movie);
  };
}

export function genres(genres: MaybeReadonlyArray<string>): SeedFn {
  return context => {
    context.client.apiv1.get('/genre').ok(genres);
  };
}

export interface MovieSeedOptions {
  movies?: MaybeReadonlyArray<DeepPartial<Movie>>;
  genres?: MaybeReadonlyArray<string>;
}

export function movies({
  movies = FAKE_MOVIES,
  genres: fakeGenres = FAKE_GENRES,
}: MovieSeedOptions = {}): SeedModule {
  return seedModule({ title: 'movies' }, ...movies.map(m => movie(m)), genres(fakeGenres));
}
