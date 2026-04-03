using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core;
using NewLoco.Web.ViewModels.Axles;
using Xunit;
using System.Collections.Generic;

namespace NewLoco.Tests;

public class AxleMeasurementServiceTests
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

    // -------------------------------
    // GetAllAsync
    // -------------------------------
    [Fact]
    public async Task GetAllAsync_ShouldReturnCards()
    {
        var db = GetDb();

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "test",
            CreatedOn = DateTime.UtcNow
        });

        db.AxleMeasurementCards.Add(new AxleMeasurementCard
        {
            Id = 1,
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 1, 1),
            AxleCount = 4,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        });

        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal(4, result[0].AxleCount);
        Assert.Equal("55-001", result[0].LocomotiveNumber);
    }

    // -------------------------------
    // GetDetailsAsync
    // -------------------------------
    [Fact]
    public async Task GetDetailsAsync_ShouldReturnCardDetails()
    {
        var db = GetDb();

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "test",
            CreatedOn = DateTime.UtcNow
        });

        db.AxleMeasurementCards.Add(new AxleMeasurementCard
        {
            Id = 1,
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 1, 1),
            AxleCount = 1,
            CreatedBy = "Tester",
            CreatedOn = DateTime.UtcNow,

            Axles =
            [
                new AxleMeasurementValue
            {
                AxleNumber = 1,
                Ar = 10,
                Sd_Left = 1,
                Sd_Right = 1,
                Sh_Left = 2,
                Sh_Right = 2,
                qR_Left = 3,
                qR_Right = 3,
                Sr = 12
            }
            ]
        });

        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);
        var details = await service.GetDetailsAsync(1);

        Assert.Equal(1, details.Id);
        Assert.Single(details.Axles);
        Assert.Equal(12, details.Axles.First().Sr);
    }

    // -------------------------------
    // GetCreateModelAsync
    // -------------------------------
    [Fact]
    public async Task GetCreateModelAsync_ShouldLoadLocomotives()
    {
        var db = GetDb();

        db.Locomotives.Add(MakeLoco(1, "55-001"));
        db.Locomotives.Add(MakeLoco(2, "52-034"));

        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);

        var model = await service.GetCreateModelAsync();

        Assert.Equal(2, model.Locomotives.Count);
        Assert.Empty(model.Axles);
    }

    // -------------------------------
    // CreateAsync
    // -------------------------------
    [Fact]
    public async Task CreateAsync_ShouldCreateCardAndAxles()
    {
        var db = GetDb();

        db.Locomotives.Add(MakeLoco(1, "55-001"));
        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);

        var model = new AxleMeasurementCardViewModel
        {
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 4, 1),
            Axles =
            [
                new AxleMeasurementValueViewModel
                {
                    AxleNumber = 1,
                    Ar = 10,
                    Sd_Left = 2,
                    Sd_Right = 3,
                    Sh_Left = 1,
                    Sh_Right = 1,
                    QR_Left = 4,
                    QR_Right = 4
                }
            ]
        };

        var id = await service.CreateAsync(model, "Tester");

        var card = await db.AxleMeasurementCards
            .Include(c => c.Axles)
            .FirstOrDefaultAsync(c => c.Id == id);

        Assert.NotNull(card);
        Assert.Single(card.Axles);
        Assert.Equal(15, card.Axles.First().Sr);
        Assert.Equal("Tester", card.CreatedBy);
    }

    // -------------------------------
    // GetEditModelAsync
    // -------------------------------
    [Fact]
    public async Task GetEditModelAsync_ShouldLoadCard()
    {
        var db = GetDb();

        db.AxleMeasurementCards.Add(new AxleMeasurementCard
        {
            Id = 10,
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 5, 1),
            CreatedBy = "test",
            CreatedOn = DateTime.UtcNow,
            Axles =
            [
                new AxleMeasurementValue
                {
                    AxleNumber = 1,
                    Ar = 5,
                    Sd_Left = 1,
                    Sd_Right = 1
                }
            ]
        });

        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);

        var model = await service.GetEditModelAsync(10);

        Assert.Equal(10, model.Id);
        Assert.Single(model.Axles);
        Assert.Equal(5, model.Axles.First().Ar);
    }

    // -------------------------------
    // UpdateAsync
    // -------------------------------
    [Fact]
    public async Task UpdateAsync_ShouldUpdateCardAndAxles()
    {
        var db = GetDb();

        db.AxleMeasurementCards.Add(new AxleMeasurementCard
        {
            Id = 10,
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 1, 1),
            CreatedBy = "test",
            CreatedOn = DateTime.UtcNow,
            Axles = []
        });

        await db.SaveChangesAsync();

        var service = new AxleMeasurementService(db);

        var model = new AxleMeasurementCardViewModel
        {
            Id = 10,
            SelectedLocomotiveId = 1,
            MeasurementDate = new DateTime(2026, 6, 1),
            Axles =
            [
                new AxleMeasurementValueViewModel
                {
                    AxleNumber = 1,
                    Ar = 10,
                    Sd_Left = 2,
                    Sd_Right = 3
                }
            ]
        };

        await service.UpdateAsync(model, "Editor");

        var card = await db.AxleMeasurementCards
            .Include(c => c.Axles)
            .FirstOrDefaultAsync(c => c.Id == 10);

        Assert.Equal(new DateTime(2026, 6, 1), card!.MeasurementDate);
        Assert.Single(card.Axles);
        Assert.Equal(15, card.Axles.First().Sr);
        Assert.Equal("Editor", card.ModifiedBy);
    }

    // -------------------------------
    // CalculateSr
    // -------------------------------
    [Fact]
    public void CalculateSr_ShouldCalculateCorrectly()
    {
        var db = GetDb();
        var service = new AxleMeasurementService(db);

        var model = new AxleMeasurementCardViewModel
        {
            Axles =
            [
                new AxleMeasurementValueViewModel
                {
                    AxleNumber = 1,
                    Ar = 10,
                    Sd_Left = 2,
                    Sd_Right = 3
                }
            ]
        };

        service.CalculateSr(model);

        Assert.Equal(15, model.Axles.First().Sr);
    }

    // -------------------------------
    // Negative tests
    // -------------------------------
    [Fact]
    public async Task GetDetailsAsync_ShouldThrow_WhenCardMissing()
    {
        var db = GetDb();
        var service = new AxleMeasurementService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetDetailsAsync(999));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenModelIsNull()
    {
        var db = GetDb();
        var service = new AxleMeasurementService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(null!, "Tester"));
    }

    [Fact]
    public async Task GetEditModelAsync_ShouldThrow_WhenCardMissing()
    {
        var db = GetDb();
        var service = new AxleMeasurementService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetEditModelAsync(555));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCardMissing()
    {
        var db = GetDb();
        var service = new AxleMeasurementService(db);

        var model = new AxleMeasurementCardViewModel { Id = 999 };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(model, "Editor"));
    }
}