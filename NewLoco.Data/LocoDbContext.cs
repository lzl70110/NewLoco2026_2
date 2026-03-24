using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;

namespace NewLoco.Data
{
    public class LocoDbContext(DbContextOptions<LocoDbContext> options) :
        IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
    {
        public virtual DbSet<Locomotive> Locomotives { get; set; } = null!;
        public virtual DbSet<ShiftWork> ShiftWorks { get; set; } = null!;
        public virtual DbSet<Fuel> Fuels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // apply IEntityTypeConfiguration<T> from this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(LocoDbContext).Assembly);

            // global soft-delete filters: hide deleted rows by default
            builder.Entity<Locomotive>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ShiftWork>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Fuel>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}