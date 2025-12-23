using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class minorchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModified",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4264));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4176));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModified",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4264),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4176),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
