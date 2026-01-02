using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedAppmemberEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMember_Tenant_TenantId",
                table: "AppMember");

            migrationBuilder.DropForeignKey(
                name: "FK_AppMember_User_Id",
                table: "AppMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_AppMember_MemberId",
                table: "Photos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppMember",
                table: "AppMember");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AppMember");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppMember");

            migrationBuilder.RenameTable(
                name: "AppMember",
                newName: "AppMembers");

            migrationBuilder.RenameIndex(
                name: "IX_AppMember_TenantId",
                table: "AppMembers",
                newName: "IX_AppMembers_TenantId");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "AppMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "AppMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppMembers",
                table: "AppMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AppMembers_CityId",
                table: "AppMembers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMembers_CountryId",
                table: "AppMembers",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMembers_City_CityId",
                table: "AppMembers",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppMembers_Country_CountryId",
                table: "AppMembers",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppMembers_Tenant_TenantId",
                table: "AppMembers",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppMembers_User_Id",
                table: "AppMembers",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_AppMembers_MemberId",
                table: "Photos",
                column: "MemberId",
                principalTable: "AppMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMembers_City_CityId",
                table: "AppMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_AppMembers_Country_CountryId",
                table: "AppMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_AppMembers_Tenant_TenantId",
                table: "AppMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_AppMembers_User_Id",
                table: "AppMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_AppMembers_MemberId",
                table: "Photos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppMembers",
                table: "AppMembers");

            migrationBuilder.DropIndex(
                name: "IX_AppMembers_CityId",
                table: "AppMembers");

            migrationBuilder.DropIndex(
                name: "IX_AppMembers_CountryId",
                table: "AppMembers");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "AppMembers");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "AppMembers");

            migrationBuilder.RenameTable(
                name: "AppMembers",
                newName: "AppMember");

            migrationBuilder.RenameIndex(
                name: "IX_AppMembers_TenantId",
                table: "AppMember",
                newName: "IX_AppMember_TenantId");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AppMember",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppMember",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppMember",
                table: "AppMember",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMember_Tenant_TenantId",
                table: "AppMember",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppMember_User_Id",
                table: "AppMember",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_AppMember_MemberId",
                table: "Photos",
                column: "MemberId",
                principalTable: "AppMember",
                principalColumn: "Id");
        }
    }
}
