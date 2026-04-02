using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAxlesCountToLocomotive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AxlesCount",
                table: "Locomotives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AxleMeasurementCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocomotiveId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    AxleCount = table.Column<int>(type: "int", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AxleMeasurementCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AxleMeasurementCards_Locomotives_LocomotiveId",
                        column: x => x.LocomotiveId,
                        principalTable: "Locomotives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AxleMeasurementValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AxleMeasurementCardId = table.Column<int>(type: "int", nullable: false),
                    AxleNumber = table.Column<int>(type: "int", nullable: false),
                    qR_Left = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    qR_Right = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sd_Left = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sd_Right = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sh_Left = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sh_Right = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AxleMeasurementValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AxleMeasurementValues_AxleMeasurementCards_AxleMeasurementCardId",
                        column: x => x.AxleMeasurementCardId,
                        principalTable: "AxleMeasurementCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AxlesCount", "CreatedOn" },
                values: new object[] { 0, new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2332) });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AxlesCount", "CreatedOn" },
                values: new object[] { 0, new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2342) });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AxlesCount", "CreatedOn" },
                values: new object[] { 0, new DateTime(2026, 3, 26, 22, 17, 20, 99, DateTimeKind.Utc).AddTicks(2346) });

            migrationBuilder.CreateIndex(
                name: "IX_AxleMeasurementCards_LocomotiveId",
                table: "AxleMeasurementCards",
                column: "LocomotiveId");

            migrationBuilder.CreateIndex(
                name: "IX_AxleMeasurementCards_Year_SequenceNumber",
                table: "AxleMeasurementCards",
                columns: new[] { "Year", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AxleMeasurementValues_AxleMeasurementCardId",
                table: "AxleMeasurementValues",
                column: "AxleMeasurementCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AxleMeasurementValues");

            migrationBuilder.DropTable(
                name: "AxleMeasurementCards");

            migrationBuilder.DropColumn(
                name: "AxlesCount",
                table: "Locomotives");

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 25, 4, 59, 49, 7, DateTimeKind.Utc).AddTicks(9249));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 25, 4, 59, 49, 7, DateTimeKind.Utc).AddTicks(9258));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 25, 4, 59, 49, 7, DateTimeKind.Utc).AddTicks(9260));
        }
    }
}
