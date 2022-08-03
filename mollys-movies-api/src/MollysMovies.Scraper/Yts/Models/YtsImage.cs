namespace MollysMovies.Scraper.Yts.Models;

/// <summary>
///     Image on the YTS service.
/// </summary>
/// <param name="Content">Raw image data.</param>
/// <param name="ContentType">Image content type e.g. 'image/png'.</param>
public record YtsImage(byte[] Content, string ContentType);