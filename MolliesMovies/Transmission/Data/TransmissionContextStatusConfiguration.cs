using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MolliesMovies.Transmission.Data
{
    public class TransmissionContextStatusConfiguration : IEntityTypeConfiguration<TransmissionContextStatus>
    {
        public void Configure(EntityTypeBuilder<TransmissionContextStatus> builder)
        {
            builder.Property(x => x.Status)
                .HasConversion(new EnumToStringConverter<TransmissionStatusCode>())
                .HasMaxLength(191).IsRequired();
        }
    }
}