import axios from 'axios';
import * as path from 'path';
import * as fs from 'fs';
import { promisify } from 'util';
import * as rimraf from 'rimraf';
import { trim } from 'lodash';
import {
  CreateStubMappingRequest,
  GetAllRequestsResponse,
  GetAllStubMappingsResponse,
  JournaledRequest,
  MatchRules,
  StubMapping,
  StubMappingRequest,
  StubMappingResponse,
} from './wiremock.types';
import { MoviePaginatedData } from '../api/model';

const rm = promisify(rimraf);
const wiremockPath = path.resolve(__dirname, 'mappings');

function urlJoin(...tokens: string[]) {
  let result = tokens
    .map(x => trim(x, '/'))
    .filter(x => x)
    .join('/');

  if (tokens.length > 0 && tokens[tokens.length - 1].endsWith('/')) {
    // special case for preserving a trailing '/'
    result += '/';
  }

  return !result.startsWith('http') ? `/${result}` : result;
}

function getUrl({
  url,
  urlPath,
  urlPattern,
  urlPathPattern,
  queryParameters,
}: StubMappingRequest): string {
  const queryTokens = Object.entries(queryParameters || {}).map(
    ([k, v]) => `${k}=${Object.values(v).join()}`,
  );
  return [
    url || urlPath || urlPattern || urlPathPattern,
    queryTokens.length === 0 ? '' : '?' + queryTokens.join('&'),
  ]
    .filter(x => x)
    .join('');
}

function getMappingName(mapping: Pick<StubMapping, 'name' | 'request' | 'response'>): string {
  return mapping.name || [mapping.request.method, getUrl(mapping.request)].join(' ');
}

type Mutation = (context: Omit<FluentWiremockContext, 'mutate'>) => void;

export type ApiName = 'apiv1';

export interface WiremockClientOptions {
  writeMappings?: boolean;
}

export class WiremockClient {
  private static readonly WIREMOCK_URL = process.env.WIREMOCK_URL || 'http://localhost:8080';
  private readonly helper: WiremockApiHelper;

  constructor({ writeMappings = false }: WiremockClientOptions = {}) {
    this.helper = new WiremockApiHelper(WiremockClient.WIREMOCK_URL, writeMappings);
  }

  setReset(value = true) {
    this.helper.setReset(value);
  }

  async getAllStubs() {
    const mappings = await this.helper.getStubs();
    return mappings.map(x => getMappingName(x));
  }

  async getRequests(basePath: string) {
    return this.helper.getRequests(basePath);
  }

  get apiv1(): FluentWiremockContext {
    return new FluentWiremockContext(this.helper, 'api/v1');
  }

  /**
   * Registers a mutation for all requests to the specified api that will be run during commit.
   */
  mutate(api: ApiName, mutation: Mutation) {
    const { basePath } = this[api];
    this.helper.addMutation(basePath, mutation);
  }

  async commit() {
    await this.helper.commit();
  }
}

class WiremockApiHelper {
  private readonly mappings: CreateStubMappingRequest[] = [];
  private readonly mutations: Record<string, Mutation[]> = {};
  private reset = true;

  constructor(public readonly wiremockUrl: string, private readonly writeMappings: boolean) {}

  stub(mapping: CreateStubMappingRequest) {
    this.mappings.push(mapping);
  }

  setReset(value: boolean) {
    this.reset = value;
  }

  async getStubs(): Promise<StubMapping[]> {
    if (this.writeMappings) {
      return [];
    }
    const { data } = await axios.get<GetAllStubMappingsResponse>(this.admin('mappings'));
    return data.mappings;
  }

  async getRequests(basePath: string): Promise<JournaledRequest[]> {
    if (this.writeMappings) {
      return [];
    }
    const url = this.admin('requests');
    const {
      data: { requests },
    } = await axios.get<GetAllRequestsResponse>(url);
    return requests.filter(x => x.request.url.startsWith(basePath));
  }

  addMutation(basePath: string, mutation: Mutation) {
    if (!this.mutations[basePath]?.push(mutation)) {
      this.mutations[basePath] = [mutation];
    }
  }

  private readonly collected: Record<
    string,
    { items: any[]; context: FluentWiremockResponseContext; fn: CollectedResponseFn<any> }
  > = {};

  collect<T>(context: FluentWiremockResponseContext, t: T, fn: CollectedResponseFn<T>) {
    const name = getMappingName(context.mapping);
    if (name in this.collected) {
      this.collected[name].items.push(t);
    } else {
      this.collected[name] = { items: [t], context, fn };
    }
  }

  async commit() {
    if (this.mappings.length === 0) {
      return;
    }

    for (const { context, items, fn } of Object.values(this.collected)) {
      fn(context, items);
    }

    for (const [basePath, mutations] of Object.entries(this.mutations)) {
      const mappings = this.mappings.filter(x => getUrl(x.request).startsWith(`/${basePath}`));
      for (const mutation of mutations) {
        for (const mapping of mappings) {
          const context = new FluentWiremockContext(this, basePath, mapping);
          mutation(context);
        }
      }
    }

    if (this.writeMappings) {
      await rm(path.join(wiremockPath, '**', '*'));
      const name = path.resolve(wiremockPath, 'mappings.json');
      const json = JSON.stringify({ mappings: this.mappings });
      await fs.promises.writeFile(name, json, { encoding: 'utf8' });
    } else {
      if (this.reset) {
        // call reset even if we're setting deleteAllNotInImport to reset request log
        await axios.post(this.admin('reset'));
      }
      await axios.post(this.admin('mappings', 'import'), {
        mappings: this.mappings,
        importOptions: {
          duplicatePolicy: 'IGNORE',
          deleteAllNotInImport: this.reset,
        },
      });
    }

    // remove all stubs
    this.mappings.splice(0, this.mappings.length);
  }

  private admin(...pathTokens: string[]): string {
    return urlJoin(this.wiremockUrl, '__admin', ...pathTokens);
  }
}

export interface FluentWiremockResponseContext {
  readonly mapping: CreateStubMappingRequest;
  ok(jsonBody?: any): void;
  notFound(jsonBody?: any): void;
  serverError(jsonBody?: any): void;
}

export type CollectedResponseFn<T> = (context: FluentWiremockResponseContext, items: T[]) => void;

export class FluentWiremockContext implements FluentWiremockResponseContext {
  regex = false;

  constructor(
    private readonly helper: WiremockApiHelper,
    readonly basePath: string,
    readonly mapping: CreateStubMappingRequest = {
      request: { method: 'GET', headers: {} },
      response: { status: 200 },
    },
  ) {}

  async getRequests(path: string) {
    return this.helper.getRequests(this.resolvePath(path));
  }

  withRegex(value = true) {
    this.regex = value;
    return this;
  }

  priority(priority: number): this {
    this.mapping.priority = priority;
    return this;
  }

  get(path: string): this {
    this.mapping.request.method = 'GET';
    this.mapping.request.urlPath = this.resolvePath(path);
    return this;
  }

  post(path: string): this {
    this.mapping.request.method = 'POST';
    this.mapping.request.urlPath = this.resolvePath(path);
    return this;
  }

  patch(path: string): this {
    this.mapping.request.method = 'PATCH';
    this.mapping.request.urlPath = this.resolvePath(path);
    return this;
  }

  basicAuth(username: string, password: string): this {
    this.mapping.request.basicAuth = { username, password };
    return this;
  }

  bearer(token: string) {
    return this.header('Authorization', `Bearer ${token}`);
  }

  header(key: string, value: string, rule: keyof MatchRules = 'equalTo'): this {
    this.mapping.request.headers!![key] = { [rule]: value };
    return this;
  }

  jsonBody(data: any): this {
    this.header('Content-Type', 'application/json');
    this.mapping.request.bodyPatterns = [{ equalToJson: data }];
    return this;
  }

  query(query: Record<string, any>, rule: keyof MatchRules = 'equalTo'): this {
    this.mapping.request.queryParameters = {
      ...this.mapping.request.queryParameters,
      ...Object.entries(query)
        .map(([k, v]) => ({ [k]: { [rule]: typeof v === 'string' ? v : v.toString() } }))
        .reduce((x, y) => ({ ...x, ...y }), {}),
    };
    return this;
  }

  queryMatches(query: Record<string, any>) {
    return this.query(query, 'matches');
  }

  collect<T>(t: T, fn: CollectedResponseFn<T>) {
    this.helper.collect(this, t, fn);
  }

  collectList<T>(t: T) {
    return this.collect(t, (c, ts) => c.ok(ts));
  }

  collectPaginated<T>(t: T) {
    return this.collect(t, (c, ts) =>
      c.ok({
        page: 1,
        limit: 10,
        count: ts.length,
        data: ts,
      } as MoviePaginatedData),
    );
  }

  ok(jsonBody?: any) {
    return this.json(200, jsonBody);
  }

  notFound(jsonBody: any = { message: 'Not found' }) {
    return this.json(404, jsonBody);
  }

  serverError(jsonBody: any = { message: 'Sorry :-(' }) {
    return this.json(500, jsonBody);
  }

  private json(status: number, jsonBody: any = {}) {
    return this.response({
      status,
      headers: { 'Content-Type': 'application/json;charset=UTF-8' },
      jsonBody,
    });
  }

  image(width: number, height: number) {
    return this.response({
      status: 200,
      headers: { 'Content-Type': 'image/jpeg' },
      base64Body: '',
    });
  }

  private resolvePath(path: string) {
    return urlJoin(this.basePath, path);
  }

  private response(response: StubMappingResponse) {
    if (this.regex) {
      this.mapping.request.urlPathPattern = this.mapping.request.urlPath;
      delete this.mapping.request.urlPath;
    }
    this.helper.stub({ ...this.mapping, response });
  }
}
