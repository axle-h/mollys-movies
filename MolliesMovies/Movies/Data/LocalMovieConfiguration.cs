using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Movies.Data
{
    public class LocalMovieConfiguration : IEntityTypeConfiguration<LocalMovie>
    {
        public void Configure(EntityTypeBuilder<LocalMovie> builder)
        {
            builder.Property(x => x.ImdbCode).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Source).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(191).IsRequired();
            builder.Property(x => x.ThumbPath).HasMaxLength(255);

            builder.HasIndex(x => x.ImdbCode).IsUnique();
            builder.HasIndex(x => x.Source);
        }
    }
}