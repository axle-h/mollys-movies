using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Movies.Data
{
    public class DownloadedMovieConfiguration : IEntityTypeConfiguration<DownloadedMovie>
    {
        public void Configure(EntityTypeBuilder<DownloadedMovie> builder)
        {
            builder.Property(x => x.MovieImdbCode).HasMaxLength(191).IsRequired();
            builder.Property(x => x.LocalMovieImdbCode).HasMaxLength(191).IsRequired();
            builder.HasOne(x => x.LocalMovie)
                .WithMany()
                .HasForeignKey(x => x.LocalMovieImdbCode)
                .HasPrincipalKey(x => x.ImdbCode)
                .IsRequired();
        }
    }
}