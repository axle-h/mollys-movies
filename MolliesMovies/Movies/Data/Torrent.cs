namespace MolliesMovies.Movies.Data
{
    public class Torrent
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        
        public string Url { get; set; }
        
        /// <summary>
        /// BitTorrent info-hash of the torrent file.
        /// Used for magnet URI's.
        /// </summary>
        public string Hash { get; set; }
        
        public string Quality { get; set; }
        
        public string Type { get; set; }
        
        public long SizeBytes { get; set; }
    }
}