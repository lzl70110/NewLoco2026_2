using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;

namespace NewLoco.Data

    {
    public class LocoDbContext : IdentityDbContext
        {
        public LocoDbContext(DbContextOptions<LocoDbContext> options)
            : base(options)
            {
            }

        protected override void OnModelCreating(ModelBuilder builder)
            {
           
            base.OnModelCreating(builder);

       
            builder.ApplyConfigurationsFromAssembly(typeof(LocoDbContext).Assembly);
            }

        public virtual DbSet<Locomotive> Locomotives { get; set; } = null!;
        public virtual DbSet<ShiftWork> ShiftWorks { get; set; } = null!;
        public virtual DbSet<Fuel> Fuels { get; set; } = null!;
        }
    }
