using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArAndSrToAxleMeasurementValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Ar",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Sr",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 27, 7, 2, 36, 820, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 27, 7, 2, 36, 820, DateTimeKind.Utc).AddTicks(1257));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 27, 7, 2, 36, 820, DateTimeKind.Utc).AddTicks(1262));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ar",
                table: "AxleMeasurementValues");

            migrationBuilder.DropColumn(
                name: "Sr",
                table: "AxleMeasurementValues");

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2332));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2342));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2346));
        }
    }
}
