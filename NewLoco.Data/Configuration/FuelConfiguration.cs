using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models.Fuel;
using static NewLoco.GCommon.EntityValidationConstants.Fuel;

namespace NewLoco.Data.Configurations
    {
    public class FuelConfiguration : IEntityTypeConfiguration<Fuel>
        {
        public void Configure(EntityTypeBuilder<Fuel> builder)
            {
            builder.ToTable("Fuels");

            builder.HasKey(f => f.Id);

            builder.HasOne(f => f.Locomotive)
                   .WithMany(l => l.Fuels)
                   .HasForeignKey(f => f.LocoId)
                   .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Property(f => f.Date).IsRequired();
            builder.Property(f => f.InitialFuel).HasColumnType(Dec).IsRequired();
            builder.Property(f => f.FinalFuel).HasColumnType(Dec).IsRequired();
            builder.Property(f => f.Consumption).HasColumnType(Dec).IsRequired();
            builder.Property(f => f.Refueled).HasColumnType(Dec).IsRequired();
            builder.Property(f => f.Note).HasMaxLength(NoteMaxLength);

            builder.HasData(
                new Fuel { Id = 1, LocoId = 1, Date = new DateOnly(2026, 1, 12), InitialFuel = 500m, FinalFuel = 400m, Consumption = 100m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 2, LocoId = 1, Date = new DateOnly(2026, 1, 13), InitialFuel = 400m, FinalFuel = 300m, Consumption = 100m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 3, LocoId = 1, Date = new DateOnly(2026, 1, 14), InitialFuel = 300m, FinalFuel = 200m, Consumption = 100m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },

                new Fuel { Id = 4, LocoId = 2, Date = new DateOnly(2026, 1, 12), InitialFuel = 1000m, FinalFuel = 975m, Consumption = 25m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 5, LocoId = 2, Date = new DateOnly(2026, 1, 13), InitialFuel = 975m, FinalFuel = 950m, Consumption = 25m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 6, LocoId = 2, Date = new DateOnly(2026, 1, 14), InitialFuel = 950m, FinalFuel = 925m, Consumption = 25m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },

                new Fuel { Id = 7, LocoId = 3, Date = new DateOnly(2026, 1, 12), InitialFuel = 1500m, FinalFuel = 1470m, Consumption = 30m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 8, LocoId = 3, Date = new DateOnly(2026, 1, 13), InitialFuel = 1470m, FinalFuel = 1440m, Consumption = 30m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false },
                new Fuel { Id = 9, LocoId = 3, Date = new DateOnly(2026, 1, 14), InitialFuel = 1440m, FinalFuel = 1410m, Consumption = 30m, Refueled = 0m, CreatedOn = DateTime.UtcNow, CreatedBy = "Seeder", IsDeleted = false }
            );
            }
        }
    }
