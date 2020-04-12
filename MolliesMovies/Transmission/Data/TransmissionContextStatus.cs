using System;

namespace MolliesMovies.Transmission.Data
{
    public class TransmissionContextStatus
    {
        public int Id { get; set; }
        
        public int TransmissionContextId { get; set; }
        
        public TransmissionStatusCode Status { get; set; }
        
        public DateTime DateCreated { get; set; }
    }
}