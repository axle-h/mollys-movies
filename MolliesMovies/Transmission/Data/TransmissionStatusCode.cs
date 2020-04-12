namespace MolliesMovies.Transmission.Data
{
    public enum TransmissionStatusCode
    {
        /// <summary>
        /// The torrent has been added to the external transmission service.
        /// </summary>
        Started = 1,
        
        /// <summary>
        /// The external transmission service has notified that the torrent is download.
        /// </summary>
        Downloaded = 2,
        
        /// <summary>
        /// The local movie scraper has run.
        /// </summary>
        Complete = 3,
    }
}