using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;

namespace NewLoco.Data.Configuration
{
    public class ShiftWorkConfiguration : IEntityTypeConfiguration<ShiftWork>
    {
        public void Configure(EntityTypeBuilder<ShiftWork> builder)
        {
            builder.HasKey(x => x.Id);
            builder
                    .HasOne(sw => sw.Locomotive)
                    .WithMany()
                    .HasForeignKey(sw => sw.LocomotiveId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Property(sw => sw.Date).HasColumnType("date");
            builder.Property(sw => sw.InitialValue).HasPrecision(18, 2);
            builder.Property(sw => sw.FinalValue).HasPrecision(18, 2);
            builder.Property(sw => sw.Amount).HasPrecision(18, 2);
        }
    }
}