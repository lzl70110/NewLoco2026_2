using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
using NewLoco.GCommon.Enums;

namespace NewLoco.Tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static LocoDbContext Create()
        {
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var ctx = new LocoDbContext(options);

            SeedLocomotives(ctx);
            SeedFuels(ctx);

            ctx.SaveChanges();

            return ctx;
        }

        private static void SeedLocomotives(LocoDbContext ctx)
        {
            if (!ctx.Locomotives.Any())
            {
                ctx.Locomotives.AddRange(
                    new Locomotive { Id = 1, Number = "06-029", LocomotiveType = LocomotiveType.Mainline },
                    new Locomotive { Id = 2, Number = "55-203", LocomotiveType = LocomotiveType.Shunter }
                );
            }
        }

        private static void SeedFuels(LocoDbContext ctx)
        {
            if (!ctx.Fuels.Any())
            {
                ctx.Fuels.AddRange(
                    new Fuel
                    {
                        Id = 101,
                        LocoId = 1,
                        Date = new DateTime(2026, 1, 12),
                        Shift = Shift.Day,
                        InitialFuel = 1410m,
                        FinalFuel = 1400m,
                        Refueled = 0m,
                        Consumption = 10m,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "seed"
                    },
                    new Fuel
                    {
                        Id = 102,
                        LocoId = 1,
                        Date = new DateTime(2026, 1, 14),
                        Shift = Shift.Day,
                        InitialFuel = 1400m,
                        FinalFuel = 1390m,
                        Refueled = 0m,
                        Consumption = 10m,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "seed"
                    },
                    new Fuel
                    {
                        Id = 201,
                        LocoId = 2,
                        Date = new DateTime(2026, 1, 13),
                        Shift = Shift.Day,
                        InitialFuel = 975m,
                        FinalFuel = 950m,
                        Refueled = 0m,
                        Consumption = 25m,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "seed"
                    }
                );
            }
        }
    }
}