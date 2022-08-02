using System.Collections.Generic;

namespace MollysMovies.Scraper.Yts.Models;

/// <summary>
///     Response from the YTS list movies API.
/// </summary>
/// <param name="MovieCount">Total count of movies available on the service.</param>
/// <param name="Limit">Requested limit.</param>
/// <param name="PageNumber">Requested page number.</param>
/// <param name="Movies">Listed movies.</param>
public record YtsListMoviesResponse(int MovieCount, int Limit, int PageNumber, ICollection<YtsMovieSummary>? Movies);