namespace MollysMovies.Scraper.Yts.Models;

/// <summary>
///     Response status wrapper used for all responses on the YTS API.
/// </summary>
/// <param name="Status">Status of the response, "ok" for success</param>
/// <param name="StatusMessage">Human readable status message.</param>
/// <param name="Data">The wrapped API response.</param>
/// <typeparam name="TData">The type of the wrapped API response.</typeparam>
public record YtsResponseWrapper<TData>(string Status, string StatusMessage, TData Data);