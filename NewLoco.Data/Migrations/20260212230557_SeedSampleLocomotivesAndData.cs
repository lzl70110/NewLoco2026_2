using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleLocomotivesAndData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Fuels",
                columns: new[] { "Id", "Consumption", "CreatedBy", "CreatedOn", "Date", "FinalFuel", "InitialFuel", "IsDeleted", "LocoId", "ModifiedBy", "ModifiedOn", "Note", "Refueled" },
                values: new object[,]
                {
                    { 1, 100m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3913), new DateOnly(2026, 1, 12), 400m, 500m, false, 1, null, null, null, 0m },
                    { 2, 100m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3920), new DateOnly(2026, 1, 13), 300m, 400m, false, 1, null, null, null, 0m },
                    { 3, 100m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3923), new DateOnly(2026, 1, 14), 200m, 300m, false, 1, null, null, null, 0m },
                    { 4, 25m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3927), new DateOnly(2026, 1, 12), 975m, 1000m, false, 2, null, null, null, 0m },
                    { 5, 25m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3930), new DateOnly(2026, 1, 13), 950m, 975m, false, 2, null, null, null, 0m },
                    { 6, 25m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3933), new DateOnly(2026, 1, 14), 925m, 950m, false, 2, null, null, null, 0m },
                    { 7, 30m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3937), new DateOnly(2026, 1, 12), 1470m, 1500m, false, 3, null, null, null, 0m },
                    { 8, 30m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3940), new DateOnly(2026, 1, 13), 1440m, 1470m, false, 3, null, null, null, 0m },
                    { 9, 30m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3943), new DateOnly(2026, 1, 14), 1410m, 1440m, false, 3, null, null, null, 0m }
                });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9951));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9955));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9957));

            migrationBuilder.InsertData(
                table: "ShiftWorks",
                columns: new[] { "Id", "Amount", "CreatedBy", "CreatedOn", "Date", "FinalValue", "InitialValue", "IsDeleted", "LocoId", "ModifiedBy", "ModifiedOn", "Note", "Shift" },
                values: new object[,]
                {
                    { 1, 5m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3723), new DateOnly(2026, 1, 12), 105m, 100m, false, 1, null, null, null, 1 },
                    { 2, 5m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3729), new DateOnly(2026, 1, 13), 110m, 105m, false, 1, null, null, null, 1 },
                    { 3, 5m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3732), new DateOnly(2026, 1, 14), 115m, 110m, false, 1, null, null, null, 1 },
                    { 4, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3735), new DateOnly(2026, 1, 12), 5050m, 5000m, false, 2, null, null, null, 1 },
                    { 5, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3738), new DateOnly(2026, 1, 13), 5100m, 5050m, false, 2, null, null, null, 1 },
                    { 6, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3742), new DateOnly(2026, 1, 14), 5150m, 5100m, false, 2, null, null, null, 1 },
                    { 7, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3745), new DateOnly(2026, 1, 12), 10050m, 10000m, false, 3, null, null, null, 1 },
                    { 8, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3748), new DateOnly(2026, 1, 13), 10100m, 10050m, false, 3, null, null, null, 1 },
                    { 9, 50m, "Seeder", new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3751), new DateOnly(2026, 1, 14), 10150m, 10100m, false, 3, null, null, null, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 22, 16, 13, 864, DateTimeKind.Utc).AddTicks(1760));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 22, 16, 13, 864, DateTimeKind.Utc).AddTicks(1768));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 2, 12, 22, 16, 13, 864, DateTimeKind.Utc).AddTicks(1772));
        }
    }
}
