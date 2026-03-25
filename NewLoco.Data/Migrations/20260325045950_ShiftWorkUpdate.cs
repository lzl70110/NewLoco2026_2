using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

 

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class ShiftWorkUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftWorks_Locomotives_LocoId",
                table: "ShiftWorks");

            migrationBuilder.DropIndex(
                name: "IX_ShiftWorks_LocoId_Date_Shift",
                table: "ShiftWorks");

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

            migrationBuilder.RenameColumn(
                name: "LocoId",
                table: "ShiftWorks",
                newName: "LocomotiveId");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialValue",
                table: "ShiftWorks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalValue",
                table: "ShiftWorks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ShiftWorks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

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

            migrationBuilder.CreateIndex(
                name: "IX_ShiftWorks_LocomotiveId",
                table: "ShiftWorks",
                column: "LocomotiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftWorks_Locomotives_LocomotiveId",
                table: "ShiftWorks",
                column: "LocomotiveId",
                principalTable: "Locomotives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftWorks_Locomotives_LocomotiveId",
                table: "ShiftWorks");

            migrationBuilder.DropIndex(
                name: "IX_ShiftWorks_LocomotiveId",
                table: "ShiftWorks");

            migrationBuilder.RenameColumn(
                name: "LocomotiveId",
                table: "ShiftWorks",
                newName: "LocoId");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialValue",
                table: "ShiftWorks",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalValue",
                table: "ShiftWorks",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "ShiftWorks",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 13, 19, 53, 55, 792, DateTimeKind.Utc).AddTicks(2795));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 13, 19, 53, 55, 792, DateTimeKind.Utc).AddTicks(2801));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 13, 19, 53, 55, 792, DateTimeKind.Utc).AddTicks(2873));

            migrationBuilder.InsertData(
                table: "ShiftWorks",
                columns: new[] { "Id", "Amount", "CreatedBy", "CreatedOn", "Date", "FinalValue", "InitialValue", "IsDeleted", "LocoId", "ModifiedBy", "ModifiedOn", "Note", "Shift" },
                values: new object[,]
                {
                    { 1, 5m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 105m, 100m, false, 1, null, null, "", 1 },
                    { 2, 5m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 110m, 105m, false, 1, null, null, "", 1 },
                    { 3, 5m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 115m, 110m, false, 1, null, null, "", 1 },
                    { 4, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 5050m, 5000m, false, 2, null, null, "", 1 },
                    { 5, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 5100m, 5050m, false, 2, null, null, "", 1 },
                    { 6, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 5150m, 5100m, false, 2, null, null, "", 1 },
                    { 7, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 10050m, 10000m, false, 3, null, null, "", 1 },
                    { 8, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 10100m, 10050m, false, 3, null, null, "", 1 },
                    { 9, 50m, "Seeder", new DateTime(2026, 3, 13, 19, 53, 55, 793, DateTimeKind.Utc).AddTicks(2117), new DateTime(2026, 1, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 10150m, 10100m, false, 3, null, null, "", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftWorks_LocoId_Date_Shift",
                table: "ShiftWorks",
                columns: new[] { "LocoId", "Date", "Shift" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftWorks_Locomotives_LocoId",
                table: "ShiftWorks",
                column: "LocoId",
                principalTable: "Locomotives",
                principalColumn: "Id");
        }
    }
}
