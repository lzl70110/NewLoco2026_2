using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Service.Core.Services;
using NewLoco.Web.ViewModels.Fuels;
using Xunit;

public class FuelServiceTests
{
    private LocoDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<LocoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LocoDbContext(options);
    }

    private IFuelService GetService(LocoDbContext db)
    {
        var policies = new FuelPoliciesOptions
        {
            DepotStepLiters = 10,
            PerClassSafety = new()
            {
                { "55", new FuelSafetyOptions { HardFloorLiters = 100, SoftWarningLiters = 120 } }
            }
        };

        var opts = Options.Create(policies);
        return new FuelService(db, opts);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Fuel_Row_With_Valid_Data()
    {
        // Arrange
        var db = GetDb();

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "test",
            CreatedOn = DateTime.UtcNow
        });

        // previous day record -> FinalFuel = 300
        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = DateTime.Today.AddDays(-1),
            Shift = Shift.Day,
            InitialFuel = 0,
            FinalFuel = 300,
            Refueled = 0,
            Consumption = 0,
            CreatedBy = "system",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = GetService(db);

        var model = new CreateFuelViewModel
        {
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 250,
            Refueled = 0,
            Note = "OK"
        };

        // Act
        await service.CreateAsync(model, "operator");

        var entity = await db.Fuels.FirstOrDefaultAsync(f => f.Date == DateTime.Today);

        // Assert
        entity.Should().NotBeNull();
        entity!.InitialFuel.Should().Be(300); // taken from previous day
        entity.FinalFuel.Should().Be(250);
        entity.Consumption.Should().Be(50);
        entity.CreatedBy.Should().Be("operator");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Date_In_Future()
    {
        var db = GetDb();
        var service = GetService(db);

        var model = new CreateFuelViewModel
        {
            LocomotiveId = 1,
            Date = DateTime.Today.AddDays(1),
            FinalFuel = 100,
            Refueled = 10
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(model, "op"));
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_FinalFuel_Is_Too_High()
    {
        var db = GetDb();

        // add locomotive
        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // previous final fuel = 200
        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = DateTime.Today.AddDays(-1),
            FinalFuel = 200,
            InitialFuel = 0,
            Refueled = 0,
            Consumption = 0,
            Shift = Shift.Day,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = GetService(db);

        var model = new CreateFuelViewModel
        {
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 250, // too high
            Refueled = 10
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(model, "op"));
    }
    [Fact]
    public async Task EditAsync_Should_Update_Fuel_Record()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 10,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 300,
            FinalFuel = 260,
            Refueled = 0,
            Consumption = 40,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 10,
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 240,
            Refueled = 0,
            Note = "Edited"
        };

        // Act
        await service.EditAsync(10, model, "user1");

        var entity = await db.Fuels.FirstAsync(f => f.Id == 10);

        // Assert
        entity.FinalFuel.Should().Be(240);
        entity.InitialFuel.Should().Be(300);
        entity.Consumption.Should().Be(60);
        entity.ModifiedBy.Should().Be("user1");
        entity.ModifiedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task EditAsync_Should_Not_Recalculate_InitialFuel()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 5,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 500,
            FinalFuel = 480,
            Refueled = 0,
            Consumption = 20,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 5,
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 460,
            Refueled = 0
        };

        await service.EditAsync(5, model, "editor");

        var entity = await db.Fuels.FirstAsync(f => f.Id == 5);

        entity.InitialFuel.Should().Be(500); // key rule
    }

    [Fact]
    public async Task EditAsync_Should_Throw_When_FinalFuel_Is_Too_High()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 20,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 300,
            FinalFuel = 260,
            Refueled = 0,
            Consumption = 40,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 20,
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 350, // too high
            Refueled = 0
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(20, model, "user"));
    }

    [Fact]
    public async Task EditAsync_Should_Throw_When_FinalFuel_Below_HardFloor()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 30,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 150,
            Refueled = 0,
            Consumption = 50,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 30,
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 80, // below hard floor (100)
            Refueled = 0
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(30, model, "user"));
    }

    [Fact]
    public async Task EditAsync_Should_Throw_When_Refueled_Not_Multiple_Of_Step()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 40,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 190,
            Refueled = 0,
            Consumption = 10,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 40,
            LocomotiveId = 1,
            Date = DateTime.Today,
            FinalFuel = 170,
            Refueled = 7 // invalid (not multiple of 10)
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(40, model, "user"));
    }

    [Fact]
    public async Task EditAsync_Should_Throw_When_Date_In_Future()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 50,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 180,
            Refueled = 0,
            Consumption = 20,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var model = new FuelAllViewModel
        {
            Id = 50,
            LocomotiveId = 1,
            Date = DateTime.Today.AddDays(1), // future
            FinalFuel = 150,
            Refueled = 0
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditAsync(50, model, "user"));
    }

    [Fact]
    public async Task RefuelAsync_Should_Add_To_Existing_Todays_Row()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 1,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 200,
            Refueled = 0,
            Consumption = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.RefuelAsync(1, 20, "user1", "added fuel");

        var entity = await db.Fuels.FirstAsync(f => f.Id == 1);

        entity.FinalFuel.Should().Be(220);
        entity.Refueled.Should().Be(20);
        entity.ModifiedBy.Should().Be("user1");
        entity.Note.Should().Contain("added fuel");
    }

    [Fact]
    public async Task RefuelAsync_Should_Create_New_Row_If_None_For_Today()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // Previous day final = 300
        db.Fuels.Add(new Fuel
        {
            Id = 10,
            LocoId = 1,
            Date = DateTime.Today.AddDays(-1),
            Shift = Shift.Day,
            InitialFuel = 0,
            FinalFuel = 300,
            Refueled = 0,
            Consumption = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.RefuelAsync(1, 40, "user1", "refuel note");

        var entity = await db.Fuels.FirstOrDefaultAsync(f => f.Date == DateTime.Today);

        entity.Should().NotBeNull();
        entity!.InitialFuel.Should().Be(300);
        entity.Refueled.Should().Be(40);
        entity.FinalFuel.Should().Be(340);
        entity.CreatedBy.Should().Be("user1");
        entity.Note.Should().Be("refuel note");
    }

    [Fact]
    public async Task RefuelAsync_Should_Throw_When_Not_Multiple_Of_Step()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RefuelAsync(1, 7, "user"));
    }

    [Fact]
    public async Task RefuelAsync_Should_Throw_When_Liters_Negative_Or_Zero()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RefuelAsync(1, 0, "user"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RefuelAsync(1, -10, "user"));
    }

    [Fact]
    public async Task RefuelAsync_Should_Append_Note_When_Existing_Note_Present()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 15,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 200,
            Refueled = 0,
            Consumption = 0,
            Note = "old",
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.RefuelAsync(1, 10, "user1", "new note");

        var result = await db.Fuels.FirstAsync(f => f.Id == 15);

        result.Note.Should().Contain("old");
        result.Note.Should().Contain("new note");
    }

    [Fact]
    public async Task ConsumeAsync_Should_Create_New_Todays_Row_When_Not_Exists()
    {
        var db = GetDb();
        var service = GetService(db);

        // Locomotive
        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // Previous record: initial source
        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = DateTime.Today.AddDays(-1),
            Shift = Shift.Day,
            InitialFuel = 0,
            FinalFuel = 300,
            Refueled = 0,
            Consumption = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.ConsumeAsync(1, 40, "user1", "note 1");

        var entity = await db.Fuels.FirstOrDefaultAsync(f => f.Date == DateTime.Today);

        // Assert
        entity.Should().NotBeNull();
        entity!.InitialFuel.Should().Be(300);
        entity.FinalFuel.Should().Be(260);
        entity.Consumption.Should().Be(40);
        entity.CreatedBy.Should().Be("user1");
        entity.Note.Should().Contain("note 1");
        entity.Shift.Should().Be(Shift.Day);
    }

    [Fact]
    public async Task ConsumeAsync_Should_Update_Existing_Todays_Row()
    {
        var db = GetDb();
        var service = GetService(db);

        // Locomotive
        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // Today's row already exists
        db.Fuels.Add(new Fuel
        {
            Id = 10,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 200,
            Consumption = 0,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.ConsumeAsync(1, 30, "user1", "added");

        var entity = await db.Fuels.FirstAsync(f => f.Id == 10);

        // Assert
        entity.FinalFuel.Should().Be(170);
        entity.Consumption.Should().Be(30);
        entity.ModifiedBy.Should().Be("user1");
        entity.Note.Should().Contain("added");
    }
    [Fact]
    public async Task ConsumeAsync_Should_Throw_When_Not_Multiple_Of_Step()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsumeAsync(1, 7, "user"));
    }

    [Fact]
    public async Task ConsumeAsync_Should_Throw_When_FinalFuel_Would_Be_Below_HardFloor()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // Today's row final = 110
        db.Fuels.Add(new Fuel
        {
            Id = 50,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 110,
            Consumption = 90,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Hard floor = 100 → consuming 20 will drop to 90
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsumeAsync(1, 20, "user"));
    }

    [Fact]
    public async Task ConsumeAsync_Should_Do_Nothing_When_Liters_Non_Positive()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 1,
            LocoId = 1,
            Date = DateTime.Today,
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 200,
            Consumption = 0,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.ConsumeAsync(1, 0, "user");  // no-op
        await service.ConsumeAsync(1, -10, "user"); // no-op

        var entity = await db.Fuels.FirstAsync();

        // Assert
        entity.FinalFuel.Should().Be(200);
        entity.Consumption.Should().Be(0);
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Create_New_Row_When_None_Exists()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // previous day: source for InitialFuel
        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = new DateTime(2026, 1, 31),
            Shift = Shift.Day,
            InitialFuel = 0,
            FinalFuel = 300,
            Consumption = 0,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.ConsumeOnAsync(
            locomotiveId: 1,
            date: new DateTime(2026, 2, 1),
            shift: Shift.Day,
            liters: 40,
            user: "op",
            note: "test"
        );

        var row = await db.Fuels.FirstOrDefaultAsync(f =>
            f.Date == new DateTime(2026, 2, 1) &&
            f.Shift == Shift.Day);

        row.Should().NotBeNull();
        row!.InitialFuel.Should().Be(300);
        row.FinalFuel.Should().Be(260);
        row.Consumption.Should().Be(40);
        row.CreatedBy.Should().Be("op");
        row.Note.Should().Contain("test");
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Update_Existing_Row()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 5,
            LocoId = 1,
            Date = new DateTime(2026, 2, 1),
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 180,
            Consumption = 20,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act
        await service.ConsumeOnAsync(1, new DateTime(2026, 2, 1), Shift.Day, 30, "user", "note");

        var entity = await db.Fuels.FirstAsync(f => f.Id == 5);

        entity.FinalFuel.Should().Be(150);
        entity.Consumption.Should().Be(50);
        entity.ModifiedBy.Should().Be("user");
        entity.Note.Should().Contain("note");
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Use_Day_Row_As_Initial_For_Night_Shift()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        // Day shift row
        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = new DateTime(2026, 3, 1),
            Shift = Shift.Day,
            InitialFuel = 300,
            FinalFuel = 260,
            Consumption = 40,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Act → should use 260 as initial
        await service.ConsumeOnAsync(1, new DateTime(2026, 3, 1), Shift.Night, 30, "user");

        var nightRow = await db.Fuels.FirstOrDefaultAsync(f =>
            f.Date == new DateTime(2026, 3, 1) &&
            f.Shift == Shift.Night);

        nightRow.Should().NotBeNull();
        nightRow!.InitialFuel.Should().Be(260);
        nightRow.FinalFuel.Should().Be(230);
        nightRow.Consumption.Should().Be(30);
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Throw_When_Not_Multiple_Of_Step()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsumeOnAsync(1, DateTime.Today, Shift.Day, 7, "user"));
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Throw_When_FinalFuel_Below_HardFloor()
    // Hard floor – Minimum safe fuel level. FinalFuel must never drop below this threshold.
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            LocoId = 1,
            Date = new DateTime(2026, 4, 1),
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 110, // hard floor = 100
            Consumption = 90,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        // Consuming 20 would drop to 90 → below hard floor → throw
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConsumeOnAsync(1, new DateTime(2026, 4, 1), Shift.Day, 20, "user"));
    }

    [Fact]
    public async Task ConsumeOnAsync_Should_Do_Nothing_When_Liters_Non_Positive()
    {
        var db = GetDb();
        var service = GetService(db);

        db.Locomotives.Add(new Locomotive
        {
            Id = 1,
            Number = "55-001",
            AxlesCount = 4,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        db.Fuels.Add(new Fuel
        {
            Id = 1,
            LocoId = 1,
            Date = new DateTime(2026, 1, 1),
            Shift = Shift.Day,
            InitialFuel = 200,
            FinalFuel = 200,
            Consumption = 0,
            Refueled = 0,
            CreatedBy = "sys",
            CreatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        await service.ConsumeOnAsync(1, new DateTime(2026, 1, 1), Shift.Day, 0, "user");
        await service.ConsumeOnAsync(1, new DateTime(2026, 1, 1), Shift.Day, -10, "user");

        var row = await db.Fuels.FirstAsync();

        row.FinalFuel.Should().Be(200);
        row.Consumption.Should().Be(0);
    }

}