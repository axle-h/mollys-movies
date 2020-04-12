
using System.Collections.Generic;
using MolliesMovies.Movies.Data;

namespace MolliesMovies.Transmission.Data
{
    public class TransmissionContext
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        
        public int TorrentId { get; set; }
        
        public int ExternalId { get; set; }
        
        public string Name { get; set; }
        
        public string MagnetUri { get; set; }
        
        public virtual ICollection<TransmissionContextStatus> Statuses { get; set; }
    }
}