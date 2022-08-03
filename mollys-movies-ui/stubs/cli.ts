import * as yargs from 'yargs';
import { orderBy } from 'lodash';
import { wiremocker } from './runner';
import { WiremockClient } from './wiremock/wiremock-client';
import { movies, MovieSeedOptions } from './movies.seed';
import { reset } from './seed.types';
import { transmission, TransmissionSeedOptions } from './transmission.seed';
import { scrapes, ScrapeSeedOptions } from './scrapes.seed';

const argv = yargs
  .option('write-mappings', {
    alias: 'w',
    type: 'boolean',
    description: 'write all wiremock mappings to disc instead',
    default: false,
    group: 'global',
  })
  .option('get-requests', {
    alias: 'm',
    type: 'string',
    description: 'get mappings for specified url and quit',
    group: 'client',
  })
  .parseSync();

type SeedOptions = MovieSeedOptions | TransmissionSeedOptions | ScrapeSeedOptions;

async function seed(args: typeof argv) {
  if (args.getRequests) {
    const client = new WiremockClient();
    const requests = await client.getRequests(args.getRequests);
    const summaries = orderBy(requests, x => x.request.loggedDate).map(x =>
      [
        x.request.loggedDateString,
        x.request.method,
        x.request.url,
        x.request.body,
        JSON.stringify(x.request.headers),
        '=>',
        x.responseDefinition.status,
      ].join(' '),
    );
    for (const summary of summaries) {
      console.log(summary);
    }
    return;
  }

  const options: SeedOptions = {};

  const modules = [
    reset,
    movies(options as MovieSeedOptions),
    transmission(options as TransmissionSeedOptions),
    scrapes(options as ScrapeSeedOptions),
  ];
  await wiremocker(modules, { writeMappings: args.writeMappings });
}

seed(argv as any)
  .then(() => process.exit(0))
  .catch(err => {
    console.error(err);
    process.exit(1);
  });
