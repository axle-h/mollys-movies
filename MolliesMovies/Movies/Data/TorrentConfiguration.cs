using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Movies.Data
{
    public class TorrentConfiguration : IEntityTypeConfiguration<Torrent>
    {
        public void Configure(EntityTypeBuilder<Torrent> builder)
        {
            builder.Property(x => x.Url).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Hash).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Quality).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Type).HasMaxLength(191).IsRequired();

            builder.HasIndex(x => x.Quality);
            builder.HasIndex(x => x.Type);
        }
    }
}