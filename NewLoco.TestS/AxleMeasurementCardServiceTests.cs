using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Service.Core;
using Xunit;

namespace NewLoco.Tests;

public class AxleMeasurementCardServiceTests
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

    // ----------------------------------------------------------
    // TEST 1: GetCreateModelAsync()
    // ----------------------------------------------------------
    [Fact]
    public async Task GetCreateModelAsync_ShouldReturnLocomotivesAndEmptyAxles()
    {
        var db = GetDb();

        db.Locomotives.Add(MakeLoco(1, "55-001"));
        db.Locomotives.Add(MakeLoco(2, "52-034"));
        await db.SaveChangesAsync();

        var service = new AxleMeasurementCardService(db);

        var model = await service.GetCreateModelAsync();

        Assert.Equal(2, model.Locomotives.Count);
        Assert.Empty(model.Axles);     
        Assert.Equal("52-034", model.Locomotives[0].Text);
        Assert.Equal("55-001", model.Locomotives[1].Text);
    }
}