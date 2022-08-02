// tslint:disable
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


import { MovieDownload } from './movie-download';

/**
 * 
 * @export
 * @interface MovieDownloadPaginatedData
 */
export interface MovieDownloadPaginatedData {
    /**
     * 
     * @type {number}
     * @memberof MovieDownloadPaginatedData
     */
    page?: number;
    /**
     * 
     * @type {number}
     * @memberof MovieDownloadPaginatedData
     */
    limit?: number;
    /**
     * 
     * @type {number}
     * @memberof MovieDownloadPaginatedData
     */
    count?: number;
    /**
     * 
     * @type {Array<MovieDownload>}
     * @memberof MovieDownloadPaginatedData
     */
    data?: Array<MovieDownload> | null;
}


