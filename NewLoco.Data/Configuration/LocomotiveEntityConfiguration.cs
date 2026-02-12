using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewLoco.Data.Models;
using NewLoco.GCommon;
using static NewLoco.GCommon.EntityValidationConstants.Locomotive;

namespace NewLoco.Data.Configurations
    {
    public class LocomotiveConfiguration : IEntityTypeConfiguration<Locomotive>
        {
        public void Configure(EntityTypeBuilder<Locomotive> builder)
            {
          
            builder.ToTable("Locomotives");

          
            builder.HasKey(l => l.Id);

  
            builder.Property(l => l.Number)
                .IsRequired()
                .HasMaxLength(LocomotiveNumberLength);

 
            builder.HasIndex(l => l.Number).IsUnique();
             
            builder.Property(l => l.LocomotiveType)
                .HasConversion<int>()
                .IsRequired();

            
            builder.Property(l => l.MeasuringUnit)
                .HasConversion<int>()  
                .IsRequired();
 
            builder.HasMany(l => l.ShiftWorks)
                .WithOne(s => s.Locomotive)
                .HasForeignKey(s => s.LocoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Fuels)
                .WithOne(f => f.Locomotive)
                .HasForeignKey(f => f.LocoId)
                .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
