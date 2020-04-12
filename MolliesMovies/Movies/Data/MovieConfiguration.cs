using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Movies.Data
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.Property(x => x.MetaSource).HasMaxLength(191).IsRequired();
            builder.Property(x => x.ImdbCode).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Language).HasMaxLength(191).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4096);
            builder.Property(x => x.Rating).HasColumnType("DECIMAL(3, 1)");
            
            builder.HasMany(x => x.MovieGenres)
                .WithOne()
                .HasForeignKey(x => x.MovieId).
                IsRequired();
            builder.HasMany(x => x.MovieSources)
                .WithOne()
                .HasForeignKey(x => x.MovieId)
                .IsRequired();
            builder.HasMany(x => x.DownloadedMovies)
                .WithOne()
                .HasForeignKey(x => x.MovieImdbCode)
                .HasPrincipalKey(x => x.ImdbCode)
                .IsRequired();
            builder.HasMany(x => x.TransmissionContexts)
                .WithOne()
                .HasForeignKey(x => x.MovieId)
                .IsRequired();
            
            builder.HasIndex(x => x.ImdbCode).IsUnique();
            builder.HasIndex(x => x.Title);
            builder.HasIndex(x => x.Rating);
            builder.HasIndex(x => x.Language);
        }
    }
}