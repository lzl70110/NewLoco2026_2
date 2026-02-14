using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuel_Index_LocoDateShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fuels_LocoId",
                table: "Fuels");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "ShiftWorks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "Shift",
                table: "Fuels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedOn", "Note", "Shift" },
                values: new object[] { new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "", 1 });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 139, DateTimeKind.Utc).AddTicks(8998), "" });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 139, DateTimeKind.Utc).AddTicks(9009), "" });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 139, DateTimeKind.Utc).AddTicks(9013), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8327), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8337), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8342), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8346), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8350), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8354), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8357), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8362), "" });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 14, 22, 48, 39, 140, DateTimeKind.Utc).AddTicks(8366), "" });

            migrationBuilder.CreateIndex(
                name: "IX_Fuels_LocoId_Date_Shift",
                table: "Fuels",
                columns: new[] { "LocoId", "Date", "Shift" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fuels_LocoId_Date_Shift",
                table: "Fuels");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "Fuels");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "ShiftWorks",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3913), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3920), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3923), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3927), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3930), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3933), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3937), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3940), null });

            migrationBuilder.UpdateData(
                table: "Fuels",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(3943), null });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9951), null });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9955), null });

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 506, DateTimeKind.Utc).AddTicks(9957), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3723), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3729), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3732), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3735), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3738), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3742), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3745), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3748), null });

            migrationBuilder.UpdateData(
                table: "ShiftWorks",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedOn", "Note" },
                values: new object[] { new DateTime(2026, 2, 12, 23, 5, 56, 507, DateTimeKind.Utc).AddTicks(3751), null });

            migrationBuilder.CreateIndex(
                name: "IX_Fuels_LocoId",
                table: "Fuels",
                column: "LocoId");
        }
    }
}
