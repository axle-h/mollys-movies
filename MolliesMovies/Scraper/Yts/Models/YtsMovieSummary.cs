using System;
using System.Collections.Generic;

namespace MolliesMovies.Scraper.Yts.Models
{
    public class YtsMovieSummary
    {
        public int Id { get; set; }
        
        public string Url { get; set; }
        
        public string ImdbCode { get; set; }
        
        public string Title { get; set; }
        
        public string TitleEnglish { get; set; }

        public string TitleLong { get; set; }
        
        public string Slug { get; set; }
        
        public int Year { get; set; }
        
        public decimal Rating { get; set; }
        
        public int Runtime { get; set; }
        
        public ICollection<string> Genres { get; set; }
        
        public string Summary { get; set; }
        
        public string DescriptionFull { get; set; }
        
        public string Synopsis { get; set; }
        
        public string YtTrailerCode { get; set; }
        
        public string Language { get; set; }
        
        public string MpaRating { get; set; }
        
        public string BackgroundImage { get; set; }
        
        public string BackgroundImageOriginal { get; set; }
        
        public string SmallCoverImage { get; set; }
        
        public string MediumCoverImage { get; set; }
        
        public string LargeCoverImage { get; set; }
        
        public string State { get; set; }
        
        public ICollection<YtsTorrent> Torrents { get; set; }
        
        public DateTime DateUploaded { get; set; }
        
        public long DateUploadedUnix { get; set; }
    }
}