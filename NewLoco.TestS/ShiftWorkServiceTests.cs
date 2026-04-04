using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;
using Xunit;

namespace NewLoco.Tests.Services
{
    public class ShiftWorkServiceTests
    {
        private LocoDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new LocoDbContext(options);

            // Seed a locomotive with required CreatedBy
            db.Locomotives.Add(new Locomotive
            {
                Id = 1,
                Number = "52-101",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "test"
            });

            db.SaveChanges();
            return db;
        }

        private ShiftWorkService GetService(LocoDbContext db)
            => new ShiftWorkService(db, new LoggerFactory().CreateLogger<ShiftWorkService>());

        [Fact]
        public async Task CreateAsync_Should_Add_ShiftWork()
        {
            var db = GetDb();
            var service = GetService(db);

            var model = new CreateShiftWorkViewModel
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialValue = 100,
                FinalValue = 150,
                Note = "Test note"
            };

            await service.CreateAsync(model, "tester");

            var sw = db.ShiftWorks.First();
            Assert.Equal(1, sw.LocomotiveId);
            Assert.Equal(50, sw.Amount);
            Assert.Equal("tester", sw.CreatedBy);
        }

        [Fact]
        public async Task EditAsync_Should_Update_ShiftWork()
        {
            var db = GetDb();
            var service = GetService(db);

            // Seed ShiftWork
            var sw = new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialValue = 100,
                FinalValue = 150,
                Amount = 50,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            };
            db.ShiftWorks.Add(sw);
            db.SaveChanges();

            var model = new EditShiftWorkViewModel
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Night,
                InitialValue = 120,
                FinalValue = 160,
                Note = "Edited"
            };

            await service.EditAsync(sw.Id, model, "editor");

            var updated = db.ShiftWorks.First();
            Assert.Equal(40, updated.Amount);
            Assert.Equal("editor", updated.ModifiedBy);
            Assert.Equal(Shift.Night, updated.Shift);
            Assert.Equal("Edited", updated.Note);
        }

        [Fact]
        public async Task DeleteAsync_Should_Mark_IsDeleted()
        {
            var db = GetDb();
            var service = GetService(db);

            var sw = new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialValue = 0,
                FinalValue = 50,
                Amount = 50,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            };
            db.ShiftWorks.Add(sw);
            db.SaveChanges();

            await service.DeleteAsync(sw.Id, "deleter");

            var deleted = db.ShiftWorks.IgnoreQueryFilters().First();
            Assert.True(deleted.IsDeleted);
            Assert.Equal("deleter", deleted.ModifiedBy);
        }

        [Fact]
        public async Task UndoDeleteAsync_Should_Unmark_IsDeleted()
        {
            var db = GetDb();
            var service = GetService(db);

            var sw = new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialValue = 0,
                FinalValue = 50,
                Amount = 50,
                IsDeleted = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            };
            db.ShiftWorks.Add(sw);
            db.SaveChanges();

            await service.UndoDeleteAsync(sw.Id, "restorer");

            var restored = db.ShiftWorks.First();
            Assert.False(restored.IsDeleted);
            Assert.Equal("restorer", restored.ModifiedBy);
        }

        [Fact]
        public async Task GetAllAsync_Should_Paginate()
        {
            var db = GetDb();
            var service = GetService(db);

            for (int i = 1; i <= 10; i++)
            {
                db.ShiftWorks.Add(new ShiftWork
                {
                    LocomotiveId = 1,
                    Date = DateTime.Today.AddDays(i),
                    Shift = Shift.Day,
                    InitialValue = i * 10,
                    FinalValue = i * 10 + 5,
                    Amount = 5,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "seed"
                });
            }
            db.SaveChanges();

            var query = new ShiftWorkQuery { Page = 2, PageSize = 3 };
            var (items, total) = await service.GetAllAsync(query);

            Assert.Equal(10, total);
            Assert.Equal(3, items.Count());
        }

        [Fact]
        public async Task GetLastShiftAsync_Should_Return_Latest()
        {
            var db = GetDb();
            var service = GetService(db);

            db.ShiftWorks.Add(new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today.AddDays(-1),
                Shift = Shift.Day,
                InitialValue = 0,
                FinalValue = 10,
                Amount = 10,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            db.ShiftWorks.Add(new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Night,
                InitialValue = 10,
                FinalValue = 20,
                Amount = 10,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            });
            db.SaveChanges();

            var last = await service.GetLastShiftAsync(1);
            Assert.Equal(DateTime.Today, last.Date);
            Assert.Equal(Shift.Night, last.Shift);
        }

        [Fact]
        public void GetForEdit_Should_Return_Model()
        {
            var db = GetDb();
            var service = GetService(db);

            var sw = new ShiftWork
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = Shift.Day,
                InitialValue = 10,
                FinalValue = 20,
                Amount = 10,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "seed"
            };
            db.ShiftWorks.Add(sw);
            db.SaveChanges();

            var model = service.GetForEdit(sw.Id);
            Assert.NotNull(model);
            Assert.Equal(sw.InitialValue, model.InitialValue);
            Assert.Equal(sw.FinalValue, model.FinalValue);
        }
    }
}