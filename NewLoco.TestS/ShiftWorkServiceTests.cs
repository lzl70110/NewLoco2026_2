using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core;
using NewLoco.Web.ViewModels.ShiftWorks;
using Xunit;

namespace NewLoco.Tests
{
    public class ShiftWorkServiceTests
    {
        private static LocoDbContext GetDb()
        {
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LocoDbContext(options);
        }

        private Locomotive MakeLoco(int id, string number)
            => new Locomotive
            {
                Id = id,
                Number = number,
                AxlesCount = 4,
                CreatedBy = "test",
                CreatedOn = DateTime.UtcNow
            };

        private ShiftWork MakeShift(int id, int locoId, DateTime date, int init, int fin, bool deleted = false)
            => new ShiftWork
            {
                Id = id,
                LocomotiveId = locoId,
                Date = date.Date,
                InitialValue = init,
                FinalValue = fin,
                Amount = fin - init,
                IsDeleted = deleted,
                CreatedBy = "test",
                CreatedOn = DateTime.UtcNow
            };

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldCreateShiftWork()
        {
            var db = GetDb();
            db.Locomotives.Add(MakeLoco(5, "52-034"));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            var model = new CreateShiftWorkViewModel
            {
                LocomotiveId = 5,
                Date = new DateTime(2026, 3, 1, 12, 30, 0),
                Shift = NewLoco.GCommon.Enums.Shift.Day,
                InitialValue = 1000,
                FinalValue = 1200,
                Note = "Test shift"
            };

            await service.CreateAsync(model, "Tester");

            var created = await db.ShiftWorks.FirstOrDefaultAsync();

            Assert.NotNull(created);
            Assert.Equal(5, created!.LocomotiveId);
            Assert.Equal(new DateTime(2026, 3, 1), created.Date);
            Assert.Equal(1000, created.InitialValue);
            Assert.Equal(1200, created.FinalValue);
            Assert.Equal(200, created.Amount);
            Assert.Equal("Tester", created.CreatedBy);
        }

        // ---------------------------------------------------------------------
        // CREATE FAIL
        // ---------------------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenFinalNotGreater()
        {
            var db = GetDb();
            db.Locomotives.Add(MakeLoco(1, "55-001"));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            var model = new CreateShiftWorkViewModel
            {
                LocomotiveId = 1,
                Date = DateTime.Today,
                Shift = NewLoco.GCommon.Enums.Shift.Day,
                InitialValue = 100,
                FinalValue = 100
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(model, "user"));
        }

        // ---------------------------------------------------------------------
        // EDIT
        // ---------------------------------------------------------------------
        [Fact]
        public async Task EditAsync_ShouldUpdate()
        {
            var db = GetDb();

            db.Locomotives.Add(MakeLoco(1, "55-001"));
            db.ShiftWorks.Add(MakeShift(10, 1, new DateTime(2026, 1, 1), 100, 150));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            var model = new EditShiftWorkViewModel
            {
                Id = 10,
                LocomotiveId = 1,
                Date = new DateTime(2026, 2, 2, 23, 10, 0),
                Shift = NewLoco.GCommon.Enums.Shift.Night,
                InitialValue = 200,
                FinalValue = 260,
                Note = "edited"
            };

            await service.EditAsync(10, model, "editor");

            var e = await db.ShiftWorks.FirstOrDefaultAsync(x => x.Id == 10);

            Assert.NotNull(e);
            Assert.Equal(new DateTime(2026, 2, 2), e!.Date);
            Assert.Equal(NewLoco.GCommon.Enums.Shift.Night, e.Shift);
            Assert.Equal(200, e.InitialValue);
            Assert.Equal(260, e.FinalValue);
            Assert.Equal(60, e.Amount);
            Assert.Equal("editor", e.ModifiedBy);
        }

        // ---------------------------------------------------------------------
        // DELETE (SOFT DELETE)
        // ---------------------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete()
        {
            var db = GetDb();

            db.Locomotives.Add(MakeLoco(1, "55-001"));
            db.ShiftWorks.Add(MakeShift(1, 1, DateTime.Today, 0, 1));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            await service.DeleteAsync(1, "u");

            var e = await db.ShiftWorks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(e);
            Assert.True(e!.IsDeleted);
            Assert.Equal("u", e.ModifiedBy);
        }

        // ---------------------------------------------------------------------
        // UNDELETE
        // ---------------------------------------------------------------------
        [Fact]
        public async Task UndoDeleteAsync_ShouldRestore()
        {
            var db = GetDb();

            db.Locomotives.Add(MakeLoco(1, "55-001"));
            db.ShiftWorks.Add(MakeShift(1, 1, DateTime.Today, 0, 1, deleted: true));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            await service.UndoDeleteAsync(1, "u");

            var e = await db.ShiftWorks
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(e);
            Assert.False(e!.IsDeleted);
            Assert.Equal("u", e.ModifiedBy);
        }

        // ---------------------------------------------------------------------
        // GET LAST SHIFT
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetLastShiftAsync_ShouldReturnLatest()
        {
            var db = GetDb();

            db.Locomotives.Add(MakeLoco(1, "55-001"));

            db.ShiftWorks.Add(MakeShift(1, 1, new DateTime(2026, 1, 1), 0, 1));
            db.ShiftWorks.Add(MakeShift(2, 1, new DateTime(2026, 1, 5), 0, 1));
            db.ShiftWorks.Add(MakeShift(3, 1, new DateTime(2026, 1, 5), 0, 1));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            var last = await service.GetLastShiftAsync(1);

            Assert.NotNull(last);
            Assert.Equal(3, last!.Id);
        }

        // ---------------------------------------------------------------------
        // FILTER + PAGING
        // ---------------------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldFilterAndPage()
        {
            var db = GetDb();

            db.Locomotives.Add(MakeLoco(1, "55-001"));
            db.Locomotives.Add(MakeLoco(2, "52-034"));

            db.ShiftWorks.Add(MakeShift(1, 1, new DateTime(2026, 1, 10), 0, 1));
            db.ShiftWorks.Add(MakeShift(2, 1, new DateTime(2026, 1, 15), 0, 1));
            db.ShiftWorks.Add(MakeShift(3, 2, new DateTime(2026, 1, 20), 0, 1));
            await db.SaveChangesAsync();

            var logger = new Mock<ILogger<ShiftWorkService>>();
            var service = new ShiftWorkService(db, logger.Object);

            var query = new ShiftWorkQuery
            {
                LocomotiveNumber = "55",
                From = new DateTime(2026, 1, 11),
                To = new DateTime(2026, 1, 30),
                Page = 1,
                PageSize = 10
            };

            var (Items, Total) = await service.GetAllAsync(query);

            Assert.Equal(1, Total);
            Assert.Single(Items);
            Assert.Equal(2, Items.First().Id);
        }
    }
}