using System;
using System.Collections.Generic;

namespace MolliesMovies.Transmission
{
    public class TransmissionOptions
    {
        public Uri RpcUri { get; set; }
        
        public ICollection<string> Trackers { get; set; }
    }
}