using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Scraper.Data
{
    public class ScrapeConfiguration : IEntityTypeConfiguration<Scrape>
    {
        public void Configure(EntityTypeBuilder<Scrape> builder)
        {
            builder.HasMany(x => x.ScrapeSources).WithOne().HasForeignKey(x => x.ScrapeId).IsRequired();
        }
    }
}