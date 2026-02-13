using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;

namespace NewLoco.Data.Configurations
    {
    public class ShiftWorkConfiguration : IEntityTypeConfiguration<ShiftWork>
        {
        public void Configure(EntityTypeBuilder<ShiftWork> builder)
            {
            builder.ToTable("ShiftWorks");

            builder.HasKey(sw => sw.Id);

            builder.HasOne(sw => sw.Locomotive)
                   .WithMany(l => l.ShiftWorks)
                   .HasForeignKey(sw => sw.LocoId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Property(sw => sw.Date).IsRequired();

            builder.Property(sw => sw.InitialValue).HasColumnType("decimal(9,2)").IsRequired();
            builder.Property(sw => sw.FinalValue).HasColumnType("decimal(9,2)").IsRequired();
            builder.Property(sw => sw.Amount).HasColumnType("decimal(9,2)").IsRequired();

            builder.Property(sw => sw.Note).HasMaxLength(1000);

            builder.HasData(
                new ShiftWork { Id = 1, LocoId = 1, Date = new DateOnly(2026, 1, 12), Shift = Shift.Day, InitialValue = 100m, FinalValue = 105m, Amount = 5m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 2, LocoId = 1, Date = new DateOnly(2026, 1, 13), Shift = Shift.Day, InitialValue = 105m, FinalValue = 110m, Amount = 5m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 3, LocoId = 1, Date = new DateOnly(2026, 1, 14), Shift = Shift.Day, InitialValue = 110m, FinalValue = 115m, Amount = 5m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },

                new ShiftWork { Id = 4, LocoId = 2, Date = new DateOnly(2026, 1, 12), Shift = Shift.Day, InitialValue = 5000m, FinalValue = 5050m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 5, LocoId = 2, Date = new DateOnly(2026, 1, 13), Shift = Shift.Day, InitialValue = 5050m, FinalValue = 5100m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 6, LocoId = 2, Date = new DateOnly(2026, 1, 14), Shift = Shift.Day, InitialValue = 5100m, FinalValue = 5150m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },

                new ShiftWork { Id = 7, LocoId = 3, Date = new DateOnly(2026, 1, 12), Shift = Shift.Day, InitialValue = 10000m, FinalValue = 10050m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 8, LocoId = 3, Date = new DateOnly(2026, 1, 13), Shift = Shift.Day, InitialValue = 10050m, FinalValue = 10100m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new ShiftWork { Id = 9, LocoId = 3, Date = new DateOnly(2026, 1, 14), Shift = Shift.Day, InitialValue = 10100m, FinalValue = 10150m, Amount = 50m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false }
            );
            }
        }
    }
