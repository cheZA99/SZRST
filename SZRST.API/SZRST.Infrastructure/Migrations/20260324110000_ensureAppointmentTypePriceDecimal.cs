using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    [DbContext(typeof(SZRSTContext))]
    [Migration("20260324110000_ensureAppointmentTypePriceDecimal")]
    public partial class ensureAppointmentTypePriceDecimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AppointmentType'
      AND COLUMN_NAME = 'Price'
      AND DATA_TYPE = 'real'
)
BEGIN
    ALTER TABLE [AppointmentType]
    ALTER COLUMN [Price] decimal(18,2) NOT NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AppointmentType'
      AND COLUMN_NAME = 'Price'
      AND DATA_TYPE IN ('decimal', 'numeric')
)
BEGIN
    ALTER TABLE [AppointmentType]
    ALTER COLUMN [Price] real NOT NULL;
END
");
        }
    }
}
