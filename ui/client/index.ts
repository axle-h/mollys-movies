import {createMakeMoviesClient} from "./makeMoviesClient";
import {FetchRequestAdapter, HttpClient} from "@microsoft/kiota-http-fetchlibrary";
import {
    AnonymousAuthenticationProvider
} from "@microsoft/kiota-abstractions/dist/cjs/src/authentication/anonymousAuthenticationProvider";
import {MovieRequestBuilderGetQueryParameters} from "./api/v1/movie";
import {ScrapeRequestBuilderGetQueryParameters} from "./api/v1/scrape";
import type {DownloadPaginatedData, Movie, MovieSummaryPaginatedData, ScrapePaginatedData} from "./models";
import useSWR, {Fetcher, SWRResponse} from 'swr'
import {DownloadRequestBuilderGetQueryParameters} from "@/client/api/v1/download";

const requestAdapter = new FetchRequestAdapter(
    new AnonymousAuthenticationProvider(),
    undefined,
    undefined,
    new HttpClient((req, init) => fetch(req, init))
)
requestAdapter.baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? ""
export const apiClient = createMakeMoviesClient(requestAdapter)

interface GetMovieParameters {
    id: string
}

const getMovie = ({ id }: GetMovieParameters) =>
    apiClient.api.v1.movie.byId(id).get();

const listMovies = (request: MovieRequestBuilderGetQueryParameters) =>
    apiClient.api.v1.movie.get({queryParameters: request});

const listScrapes = (request: ScrapeRequestBuilderGetQueryParameters) =>
    apiClient.api.v1.scrape.get({queryParameters: request});

const listDownloads = (request: DownloadRequestBuilderGetQueryParameters) =>
    apiClient.api.v1.download.get({queryParameters: request});

export type ApiName = 'list-scrapes' | 'list-movies' | 'get-movie' | 'list-downloads'

export interface ApiRequest<T extends ApiName> { api: T }

export type ListMoviesRequest = ApiRequest<'list-movies'> & MovieRequestBuilderGetQueryParameters
export type ListScrapesRequest = ApiRequest<'list-scrapes'> & ScrapeRequestBuilderGetQueryParameters
export type GetMovieRequest = ApiRequest<'get-movie'> & GetMovieParameters
export type ListDownloadsRequest = ApiRequest<'list-downloads'> & DownloadRequestBuilderGetQueryParameters
export type Request = ListMoviesRequest | ListScrapesRequest | GetMovieRequest | ListDownloadsRequest

export function useClient(request: ListMoviesRequest):  SWRResponse<MovieSummaryPaginatedData>
export function useClient(request: ListScrapesRequest):  SWRResponse<ScrapePaginatedData>
export function useClient(request: GetMovieRequest):  SWRResponse<Movie>
export function useClient(request: ListDownloadsRequest):  SWRResponse<DownloadPaginatedData>
export function useClient(request: Request):  SWRResponse {
    let fetcher: any;
    switch (request.api) {
        case "list-movies":
            fetcher = listMovies;
            break;
        case "list-scrapes":
            fetcher = listScrapes;
            break;
        case "get-movie":
            fetcher = getMovie;
            break;
        case "list-downloads":
            fetcher = listDownloads;
            break;
    }

    return useSWR(request, fetcher);
}