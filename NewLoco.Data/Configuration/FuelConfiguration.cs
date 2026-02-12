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

            builder.Property(f => f.InitialFuel)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(f => f.FinalFuel)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(f => f.Consumption)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(f => f.Refueled)
                   .HasColumnType(Dec)
                   .IsRequired();

            builder.Property(f => f.Note)
                   .HasMaxLength(NoteMaxLength);
            }
        }
    }
