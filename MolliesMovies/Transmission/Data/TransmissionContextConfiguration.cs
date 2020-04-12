using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MolliesMovies.Transmission.Data
{
    public class TransmissionContextConfiguration : IEntityTypeConfiguration<TransmissionContext>
    {
        public void Configure(EntityTypeBuilder<TransmissionContext> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            builder.Property(x => x.MagnetUri).HasMaxLength(4096).IsRequired();

            builder.HasMany(x => x.Statuses)
                .WithOne()
                .HasForeignKey(x => x.TransmissionContextId)
                .IsRequired();

            builder.HasIndex(x => x.ExternalId);
        }
    }
}