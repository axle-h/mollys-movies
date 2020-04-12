using MolliesMovies.Transmission.Data;

namespace MolliesMovies.Transmission.Models
{
    public class TransmissionContextDto
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        
        public int TorrentId { get; set; }
        
        public int ExternalId { get; set; }
        
        public string Name { get; set; }
        
        public string MagnetUri { get; set; }
        
        public TransmissionStatusCode Status { get; set; }
    }
}