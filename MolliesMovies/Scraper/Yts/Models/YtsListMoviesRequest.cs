namespace MolliesMovies.Scraper.Yts.Models
{
    public class YtsListMoviesRequest
    {
        /// <summary>
        /// The limit of results per page that has been set.
        /// Integer between 1 - 50 (inclusive)
        /// Default 20
        /// </summary>
        public int? Limit { get; set; }
        
        /// <summary>
        /// Used to see the next page of movies, eg limit=15 and page=2 will show you movies 15-30.
        /// Integer (Unsigned)
        /// Default 1
        /// </summary>
        public int? Page { get; set; }
        
        /// <summary>
        /// Used to filter by a given quality.
        /// String (720p, 1080p, 2160p, 3D)
        /// Default All
        /// </summary>
        public string Quality { get; set; }
        
        /// <summary>
        /// Used to filter movie by a given minimum IMDb rating.
        /// Integer between 0 - 9 (inclusive)
        /// Default 0
        /// </summary>
        public int? MinimumRating { get; set; }
        
        /// <summary>
        /// Used for movie search, matching on: Movie Title/IMDb Code, Actor Name/IMDb Code, Director Name/IMDb Code.
        /// </summary>
        public string QueryTerm { get; set; }
        
        /// <summary>
        /// Used to filter by a given genre (See http://www.imdb.com/genre/ for full list).
        /// </summary>
        public string Genre { get; set; }
        
        /// <summary>
        /// Sorts the results by chosen value.
        /// String (title, year, rating, peers, seeds, download_count, like_count, date_added)
        /// Default date_added
        /// </summary>
        public string SortBy { get; set; }
        
        /// <summary>
        /// Orders the results by either Ascending or Descending order.
        /// String (desc, asc)
        /// Default desc
        /// </summary>
        public string OrderBy { get; set; }
        
        /// <summary>
        /// Returns the list with the Rotten Tomatoes rating included.
        /// </summary>
        public bool? WithRtRatings { get; set; }
    }
}