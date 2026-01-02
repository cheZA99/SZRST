using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedAppmemberEntity_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMembers_Tenant_TenantId",
                table: "AppMembers");

            migrationBuilder.DropIndex(
                name: "IX_AppMembers_TenantId",
                table: "AppMembers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AppMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AppMembers_TenantId",
                table: "AppMembers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMembers_Tenant_TenantId",
                table: "AppMembers",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
