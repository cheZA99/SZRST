using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    public partial class addedValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModified",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4264),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 11, 34, 982, DateTimeKind.Utc).AddTicks(5779));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4176),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 11, 34, 982, DateTimeKind.Utc).AddTicks(5631));

            migrationBuilder.InsertData(
        table: "FacilityType",
        columns: new[] { "Id", "Name", "Description", "DateCreated", "IsDeleted" },
        values: new object[,]
        {
            { 1, "Padel", "Padel court for games and practice", DateTime.UtcNow, false },
            { 2, "Tennis", "Tennis court for singles and doubles matches", DateTime.UtcNow, false },
            { 3, "Futsal", "Indoor or outdoor futsal field", DateTime.UtcNow, false },
            { 4, "Basketball", "Basketball court for matches and training", DateTime.UtcNow, false },
            { 5, "Volleyball", "Volleyball court for indoor and beach games", DateTime.UtcNow, false }
        }
    );

            // Insert Currencies First
            migrationBuilder.InsertData(
                table: "Currency",
                columns: new[] { "Id", "Name", "ShortName", "DateCreated", "IsDeleted" },
                values: new object[,]
                {
            { 1, "Euro", "EUR", DateTime.UtcNow, false },
            { 2, "Serbian Dinar", "RSD", DateTime.UtcNow, false },
            { 3, "Bosnian Convertible Mark", "BAM", DateTime.UtcNow, false }
                }
            );

            // Insert Countries with CurrencyId instead of Currency object
            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "Name", "ShortName", "CurrencyId", "DateCreated", "IsDeleted" },
                values: new object[,]
                {
            { 1, "Croatia", "HR", 1, DateTime.UtcNow, false }, // Uses EUR (Id=1)
            { 2, "Serbia", "RS", 2, DateTime.UtcNow, false },  // Uses RSD (Id=2)
            { 3, "Bosnia", "BA", 3, DateTime.UtcNow, false },  // Uses BAM (Id=3)
            { 4, "Austria", "AT", 1, DateTime.UtcNow, false }  // Uses EUR (Id=1)
                }
            );

            migrationBuilder.InsertData(
        table: "City",
        columns: new[] { "Id", "Name", "CountryId", "DateCreated", "IsDeleted" },
        values: new object[,]
        {
            { 1, "Zagreb", 1, DateTime.UtcNow, false },   // Croatia
            { 2, "Split", 1, DateTime.UtcNow, false },    // Croatia
            { 3, "Belgrade", 2, DateTime.UtcNow, false }, // Serbia
            { 4, "Novi Sad", 2, DateTime.UtcNow, false }, // Serbia
            { 5, "Sarajevo", 3, DateTime.UtcNow, false }, // Bosnia
            { 6, "Banja Luka", 3, DateTime.UtcNow, false }, // Bosnia
            { 7, "Vienna", 4, DateTime.UtcNow, false },   // Austria
            { 8, "Graz", 4, DateTime.UtcNow, false }      // Austria
        }
    );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModified",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 11, 34, 982, DateTimeKind.Utc).AddTicks(5779),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4264));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "UserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 7, 19, 11, 34, 982, DateTimeKind.Utc).AddTicks(5631),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 2, 7, 19, 12, 50, 379, DateTimeKind.Utc).AddTicks(4176));
        }
    }
}
