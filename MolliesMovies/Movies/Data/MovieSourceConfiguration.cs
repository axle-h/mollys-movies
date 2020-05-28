using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Movies.Data
{
    public class MovieSourceConfiguration : IEntityTypeConfiguration<MovieSource>
    {
        public void Configure(EntityTypeBuilder<MovieSource> builder)
        {
            builder.Property(x => x.Source).HasMaxLength(191).IsRequired();
            builder.Property(x => x.SourceUrl).HasMaxLength(255).IsRequired();
            builder.Property(x => x.SourceId).HasMaxLength(191).IsRequired();
            builder.Property(x => x.SourceCoverImageUrl).HasMaxLength(255);
            builder.Property(x => x.DateCreated).IsRequired();
            builder.Property(x => x.DateScraped).IsRequired();
            
            builder.HasMany(x => x.Torrents)
                .WithOne()
                .HasForeignKey(x => x.MovieId)
                .IsRequired();
            
            builder.HasIndex(x => x.Source);
            builder.HasIndex(x => x.DateCreated);
        }
    }
}