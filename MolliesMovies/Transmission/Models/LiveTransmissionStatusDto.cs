namespace MolliesMovies.Transmission.Models
{
    public class LiveTransmissionStatusDto
    {
        public string Name { get; set; }
        
        /// <summary>
        /// Whether the download is complete.
        /// </summary>
        public bool Complete { get; set; }
        
        /// <summary>
        /// Whether the download is stalled due to lack of seeds.
        /// </summary>
        public bool? Stalled { get; set; }
        
        /// <summary>
        /// The total estimated seconds until this download is done.
        /// </summary>
        public int? Eta { get; set; }
        
        /// <summary>
        /// The percentage complete 0...1.
        /// </summary>
        public double? PercentComplete { get; set; }

        public static LiveTransmissionStatusDto GetComplete(string name) =>
            new LiveTransmissionStatusDto {Complete = true, Name = name};
    }
}