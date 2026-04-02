using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;

namespace NewLoco.Data.Configuration;

public class AxleMeasurementCardConfiguration : IEntityTypeConfiguration<AxleMeasurementCard>
{
    public void Configure(EntityTypeBuilder<AxleMeasurementCard> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Locomotive)
            .WithMany()
            .HasForeignKey(x => x.SelectedLocomotiveId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.MeasurementDate)
            .IsRequired();

        builder
            .HasMany(x => x.Axles)
            .WithOne(x => x.AxleMeasurementCard)
            .HasForeignKey(x => x.AxleMeasurementCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}