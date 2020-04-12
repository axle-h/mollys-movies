using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MolliesMovies.Scraper.Data
{
    public class ScrapeSourceConfiguration : IEntityTypeConfiguration<ScrapeSource>
    {
        public void Configure(EntityTypeBuilder<ScrapeSource> builder)
        {
            builder.Property(x => x.Source).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Type).HasConversion(new EnumToStringConverter<ScraperType>()).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Error).HasMaxLength(4096);
        }
    }
}