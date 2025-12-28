using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removedTenantFromLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_Tenant_TenantId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_TenantId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Location",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Location_TenantId",
                table: "Location",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Tenant_TenantId",
                table: "Location",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
