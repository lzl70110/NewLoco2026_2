using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewLoco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSelectedLocomotiveIdToAxleMeasuremetCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AxleMeasurementCards_Locomotives_LocomotiveId",
                table: "AxleMeasurementCards");

            migrationBuilder.RenameColumn(
                name: "LocomotiveId",
                table: "AxleMeasurementCards",
                newName: "SelectedLocomotiveId");

            migrationBuilder.RenameIndex(
                name: "IX_AxleMeasurementCards_LocomotiveId",
                table: "AxleMeasurementCards",
                newName: "IX_AxleMeasurementCards_SelectedLocomotiveId");

            migrationBuilder.AlterColumn<double>(
                name: "qR_Right",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "qR_Left",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sr",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sh_Right",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sh_Left",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sd_Right",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sd_Left",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Ar",
                table: "AxleMeasurementValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 31, 16, 17, 40, 735, DateTimeKind.Utc).AddTicks(7052));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 31, 16, 17, 40, 735, DateTimeKind.Utc).AddTicks(7058));

            migrationBuilder.UpdateData(
                table: "Locomotives",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedOn",
                value: new DateTime(2026, 3, 31, 16, 17, 40, 735, DateTimeKind.Utc).AddTicks(7062));

            migrationBuilder.AddForeignKey(
                name: "FK_AxleMeasurementCards_Locomotives_SelectedLocomotiveId",
                table: "AxleMeasurementCards",
                column: "SelectedLocomotiveId",
                principalTable: "Locomotives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AxleMeasurementCards_Locomotives_SelectedLocomotiveId",
                table: "AxleMeasurementCards");

            migrationBuilder.RenameColumn(
                name: "SelectedLocomotiveId",
                table: "AxleMeasurementCards",
                newName: "LocomotiveId");

            migrationBuilder.RenameIndex(
                name: "IX_AxleMeasurementCards_SelectedLocomotiveId",
                table: "AxleMeasurementCards",
                newName: "IX_AxleMeasurementCards_LocomotiveId");

            migrationBuilder.AlterColumn<decimal>(
                name: "qR_Right",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "qR_Left",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sr",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sh_Right",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sh_Left",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sd_Right",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sd_Left",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ar",
                table: "AxleMeasurementValues",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_AxleMeasurementCards_Locomotives_LocomotiveId",
                table: "AxleMeasurementCards",
                column: "LocomotiveId",
                principalTable: "Locomotives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
