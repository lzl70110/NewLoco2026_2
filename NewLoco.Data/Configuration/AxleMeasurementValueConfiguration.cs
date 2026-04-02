using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;

namespace NewLoco.Data.Configuration;

public class AxleMeasurementValueConfiguration : IEntityTypeConfiguration<AxleMeasurementValue>
{
    public void Configure(EntityTypeBuilder<AxleMeasurementValue> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.AxleMeasurementCard)
            .WithMany(x => x.Axles)
            .HasForeignKey(x => x.AxleMeasurementCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.AxleNumber)
            .IsRequired();
    }
}