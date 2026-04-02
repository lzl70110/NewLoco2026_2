using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Service.Core.Services;
using NewLoco.Web.ViewModels.Fuels;
using Xunit;

namespace NewLoco.Tests
{
    public class FuelServiceTests
    {
        private static LocoDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseInMemoryDatabase($"FuelDb_{Guid.NewGuid()}")
                .Options;

            return new LocoDbContext(options);
        }

        private static void SeedLocomotive(LocoDbContext db, int id)
        {
            db.Locomotives.Add(new Locomotive
            {
                Id = id,
                Number = $"L{id}",
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                AxlesCount = 4,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });

            db.SaveChanges();
        }

        private static IFuelService CreateService(LocoDbContext db)
        {
            var policies = new FuelPoliciesOptions
            {
                DepotStepLiters = 10,
                Shunter = new FuelPolicy
                {
                    MinIdleLph = 5,
                    MinLoadLph = 8,
                    FullLoadLphHint = 9
                },
                Mainline = new FuelPolicy
                {
                    MinIdleLph = 6,
                    MinLoadLph = 10,
                    FullLoadLphHint = 12
                },
                PerClassSafety = new Dictionary<string, FuelSafetyOptions>
                {
                    { "L", new FuelSafetyOptions { SoftWarningLiters = 120, HardFloorLiters = 100 } }
                }
            };

            return new FuelService(db, Options.Create(policies));
        }

        // --------------------- CreateAsync Tests ---------------------

        [Fact]
        public async Task CreateAsync_Should_Create_When_ValidInput()
        {
            var db = CreateDb();
            SeedLocomotive(db, 5);
            var service = CreateService(db);

            db.Fuels.Add(new Fuel
            {
                LocoId = 5,
                Locomotive = await db.Locomotives.FirstAsync(l => l.Id == 5),
                Date = DateTime.Today.AddDays(-1),
                Shift = Shift.Day,
                InitialFuel = 200,
                FinalFuel = 180,
                Consumption = 20,
                Refueled = 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });

            await db.SaveChangesAsync();

            var model = new CreateFuelViewModel
            {
                LocomotiveId = 5,
                Date = DateTime.Today,
                FinalFuel = 150,
                Refueled = 30
            };

            await service.CreateAsync(model, "tester");

            var created = await db.Fuels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.LocoId == 5 && f.Date == DateTime.Today);

            created.Should().NotBeNull();
            created!.InitialFuel.Should().Be(180);
            created.Refueled.Should().Be(30);
            created.FinalFuel.Should().Be(150);
            created.Consumption.Should().Be(60);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_DateInFuture()
        {
            var db = CreateDb();
            SeedLocomotive(db, 1);
            var service = CreateService(db);

            var model = new CreateFuelViewModel
            {
                LocomotiveId = 1,
                Date = DateTime.Today.AddDays(1),
                FinalFuel = 100,
                Refueled = 10
            };

            Func<Task> act = async () => await service.CreateAsync(model, "tester");

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_FinalFuelTooHigh()
        {
            var db = CreateDb();
            SeedLocomotive(db, 5);
            var service = CreateService(db);

            db.Fuels.Add(new Fuel
            {
                LocoId = 5,
                Locomotive = await db.Locomotives.FirstAsync(l => l.Id == 5),
                Date = DateTime.Today.AddDays(-1),
                Shift = Shift.Day,
                InitialFuel = 200,
                FinalFuel = 180,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });

            await db.SaveChangesAsync();

            var model = new CreateFuelViewModel
            {
                LocomotiveId = 5,
                Date = DateTime.Today,
                Refueled = 20,
                FinalFuel = 250
            };

            Func<Task> act = async () => await service.CreateAsync(model, "tester");

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // --------------------- DeleteAsync ---------------------

        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_FuelRecord()
        {
            var db = CreateDb();
            SeedLocomotive(db, 3);
            var service = CreateService(db);

            var loco = await db.Locomotives.FirstAsync(l => l.Id == 3);

            var fuel = new Fuel
            {
                LocoId = 3,
                Locomotive = loco,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialFuel = 300,
                FinalFuel = 250,
                Consumption = 50,
                Refueled = 0,
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow
            };

            db.Fuels.Add(fuel);
            await db.SaveChangesAsync();

            await service.DeleteAsync(fuel.Id, "tester");

            var result = await db.Fuels
                .IgnoreQueryFilters()
                .FirstAsync(f => f.Id == fuel.Id);

            result.IsDeleted.Should().BeTrue();
            result.ModifiedBy.Should().Be("tester");
            result.ModifiedOn.Should().NotBeNull();
        }

        // --------------------- UndoDeleteAsync ---------------------

        [Fact]
        public async Task UndoDeleteAsync_Should_Restore_FuelRecord()
        {
            var db = CreateDb();
            SeedLocomotive(db, 4);
            var service = CreateService(db);

            var loco = await db.Locomotives.FirstAsync(l => l.Id == 4);

            var fuel = new Fuel
            {
                LocoId = 4,
                Locomotive = loco,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialFuel = 500,
                FinalFuel = 450,
                Consumption = 50,
                Refueled = 0,
                CreatedBy = "seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = true
            };

            db.Fuels.Add(fuel);
            await db.SaveChangesAsync();

            await service.UndoDeleteAsync(fuel.Id, "tester");

            var result = await db.Fuels
                .IgnoreQueryFilters()
                .FirstAsync(f => f.Id == fuel.Id);

            result.IsDeleted.Should().BeFalse();
            result.ModifiedBy.Should().Be("tester");
            result.ModifiedOn.Should().NotBeNull();
        }

        // --------------------- GetPrevFinalAsync ---------------------

        [Fact]
        public async Task GetPrevFinalAsync_Should_Return_Last_FinalFuel_Before_Date()
        {
            var db = CreateDb();
            SeedLocomotive(db, 7);
            var service = CreateService(db);

            var loco = await db.Locomotives.FirstAsync(l => l.Id == 7);

            db.Fuels.AddRange(
                new Fuel
                {
                    LocoId = 7,
                    Locomotive = loco,
                    Date = new DateTime(2026, 1, 10),
                    FinalFuel = 480,
                    InitialFuel = 500,
                    Consumption = 20,
                    Refueled = 0,
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Shift = Shift.Day
                },
                new Fuel
                {
                    LocoId = 7,
                    Locomotive = loco,
                    Date = new DateTime(2026, 1, 11),
                    FinalFuel = 450,
                    InitialFuel = 480,
                    Consumption = 30,
                    Refueled = 0,
                    CreatedBy = "seed",
                    CreatedOn = DateTime.UtcNow,
                    Shift = Shift.Day
                }
            );

            await db.SaveChangesAsync();

            var result = await service.GetPrevFinalAsync(7, new DateTime(2026, 1, 12));

            result.Should().Be(450);
        }
    }
}