/**
 * Public Molly's Movies API
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * 
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { LocalMovieSource } from './local-movie-source';
import { Torrent } from './torrent';
import { MovieDownload } from './movie-download';


export interface Movie { 
    imdbCode?: string | null;
    title?: string | null;
    language?: string | null;
    year?: number;
    rating?: number | null;
    description?: string | null;
    youTubeTrailerCode?: string | null;
    imageFilename?: string | null;
    genres?: Array<string> | null;
    torrents?: Array<Torrent> | null;
    localSource?: LocalMovieSource;
    download?: MovieDownload;
}

