using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;
using static NewLoco.GCommon.EntityValidationConstants.ShiftWork;

namespace NewLoco.Data.Configurations
    {
    public class ShiftWorkConfiguration : IEntityTypeConfiguration<ShiftWork>
        {
        public void Configure(EntityTypeBuilder<ShiftWork> builder)
            {
            builder.ToTable("ShiftWorks");

            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Locomotive)
                   .WithMany(l => l.ShiftWorks)
                   .HasForeignKey(s => s.LocoId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Property(s => s.Date).IsRequired();

            builder.Property(s => s.Shift)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(s => s.InitialValue)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(s => s.FinalValue)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(s => s.Amount)
                   .HasColumnType(Dec)
                   .IsRequired();
            }
        }
    }
