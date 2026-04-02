using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core;
using NewLoco.Service.Core.Contracts;
using Xunit;

namespace NewLoco.Tests
{
    public class LocomotiveServiceTests
    {
        private static LocoDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseInMemoryDatabase($"LocoTestDb_{Guid.NewGuid()}")
                .Options;

            return new LocoDbContext(options);
        }

        // ----------------------------------------------------------
        // TEST 1: CreateAsync()
        // ----------------------------------------------------------
        [Fact]
        public async Task CreateAsync_Should_Add_New_Locomotive()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var user = "TestUser";

            var dto = new LocomotiveFormDto(
                Number: "52034",
                LocomotiveType: LocomotiveType.Shunter,
                MeasuringUnit: MeasuringUnits.Mh,
                AxlesCount: 4,
                Note: "Test loco"
            );

            // Act
            await service.CreateAsync(dto, user);

            // Assert
            var locomotive = await context.Locomotives.FirstOrDefaultAsync();

            locomotive.Should().NotBeNull();
            locomotive!.Number.Should().Be("52034");
            locomotive.AxlesCount.Should().Be(4);
            locomotive.LocomotiveType.Should().Be(LocomotiveType.Shunter);
            locomotive.CreatedBy.Should().Be("TestUser");
            locomotive.IsDeleted.Should().BeFalse();
        }

        // ----------------------------------------------------------
        // TEST 2: GetDetailsAsync()
        // ----------------------------------------------------------
        [Fact]
        public async Task GetDetailsAsync_Should_Return_Details()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco = new Locomotive
            {
                Number = "55001",
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Km,
                AxlesCount = 6,
                CreatedBy = "Tester",
                CreatedOn = DateTime.UtcNow
            };

            context.Locomotives.Add(loco);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetDetailsAsync(loco.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Number.Should().Be("55001");
            result.AxlesCount.Should().Be(6);
            result.LocomotiveType.Should().Be(LocomotiveType.Shunter);
        }

        // ----------------------------------------------------------
        // TEST 3: EditAsync()
        // ----------------------------------------------------------
        [Fact]
        public async Task EditAsync_Should_Modify_Locomotive()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco = new Locomotive
            {
                Number = "52000",
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                AxlesCount = 4,
                CreatedBy = "A",
                CreatedOn = DateTime.UtcNow
            };

            context.Locomotives.Add(loco);
            await context.SaveChangesAsync();

            var dto = new LocomotiveFormDto(
                Number: "52000-EDIT",
                LocomotiveType: LocomotiveType.Shunter,
                MeasuringUnit: MeasuringUnits.Km,
                AxlesCount: 8,
                Note: "Updated"
            );

            // Act
            await service.EditAsync(loco.Id, dto, "Editor");

            // Assert
            loco.Number.Should().Be("52000-EDIT");
            loco.MeasuringUnit.Should().Be(MeasuringUnits.Km);
            loco.AxlesCount.Should().Be(8);
            loco.ModifiedBy.Should().Be("Editor");
        }

        // ----------------------------------------------------------
        // TEST 4: DeleteAsync()
        // ----------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_Should_SoftDelete()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco = new Locomotive
            {
                Number = "12345",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            context.Locomotives.Add(loco);
            await context.SaveChangesAsync();

            // Act
            await service.DeleteAsync(loco.Id, "Admin");

            // Assert
            loco.IsDeleted.Should().BeTrue();
            loco.ModifiedBy.Should().Be("Admin");
            loco.ModifiedOn.Should().NotBeNull();
        }

        // ----------------------------------------------------------
        // TEST 5: UndeleteAsync()
        // ----------------------------------------------------------
        [Fact]
        public async Task UndeleteAsync_Should_Restore()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco = new Locomotive
            {
                Number = "99999",
                IsDeleted = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            context.Locomotives.Add(loco);
            await context.SaveChangesAsync();

            // Act
            await service.UndeleteAsync(loco.Id, "Admin");

            // Assert
            loco.IsDeleted.Should().BeFalse();
            loco.ModifiedBy.Should().Be("Admin");
            loco.ModifiedOn.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Active()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var active1 = new Locomotive
            {
                Number = "52001",
                AxlesCount = 4,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var active2 = new Locomotive
            {
                Number = "52002",
                AxlesCount = 6,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var deleted = new Locomotive
            {
                Number = "99999",
                AxlesCount = 8,
                IsDeleted = true,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            context.Locomotives.AddRange(active1, active2, deleted);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync(null);

            // Assert
            result.Should().HaveCount(2);
            result.All(l => !l.IsDeleted).Should().BeTrue();
        }
        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Deleted_When_Filter_Is_Deleted()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var active1 = new Locomotive
            {
                Number = "70001",
                AxlesCount = 4,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var active2 = new Locomotive
            {
                Number = "70002",
                AxlesCount = 6,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var deleted1 = new Locomotive
            {
                Number = "DELETED-001",
                AxlesCount = 8,
                IsDeleted = true,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Km,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            context.Locomotives.AddRange(active1, active2, deleted1);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync("deleted");

            // Assert
            result.Should().HaveCount(1);
            result.First().Number.Should().Be("DELETED-001");
            result.All(l => l.IsDeleted).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_When_Filter_Is_All()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco1 = new Locomotive
            {
                Number = "10001",
                AxlesCount = 4,
                IsDeleted = false,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var loco2 = new Locomotive
            {
                Number = "10002",
                AxlesCount = 6,
                IsDeleted = true, // deleted
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Km,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            var loco3 = new Locomotive
            {
                Number = "10003",
                AxlesCount = 8,
                IsDeleted = false,
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Mh,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "User"
            };

            context.Locomotives.AddRange(loco1, loco2, loco3);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllAsync("all");

            // Assert
            result.Should().HaveCount(3);
  
            result.Any(l => l.IsDeleted).Should().BeTrue();
            result.Any(l => !l.IsDeleted).Should().BeTrue();

            result.Select(r => r.Number).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetForEditAsync_Should_Return_DTO_When_Valid_Id()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            var loco = new Locomotive
            {
                Number = "51000",
                LocomotiveType = LocomotiveType.Shunter,
                MeasuringUnit = MeasuringUnits.Km,
                AxlesCount = 6,
                Note = "Edit me",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "UnitTest"
            };

            context.Locomotives.Add(loco);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetForEditAsync(loco.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Number.Should().Be("51000");
            result.AxlesCount.Should().Be(6);
            result.Note.Should().Be("Edit me");
            result.MeasuringUnit.Should().Be(MeasuringUnits.Km);
        }

        [Fact]
        public async Task GetForEditAsync_Should_Return_Null_When_Invalid_Id()
        {
            // Arrange
            var context = CreateDbContext();
            var service = new LocomotiveService(context);

            // Act
            var result = await service.GetForEditAsync(9999); // false Id

            // Assert
            result.Should().BeNull();
        }

    }

}