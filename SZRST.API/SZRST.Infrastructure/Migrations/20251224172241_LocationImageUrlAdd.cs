using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationImageUrlAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Location");
        }
    }
}
