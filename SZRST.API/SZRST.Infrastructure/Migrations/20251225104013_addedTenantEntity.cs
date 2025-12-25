using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SZRST.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedTenantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "WorkerType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Worker",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Reservation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Location",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Facility",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AppointmentType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Appointment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AppMember",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerType_TenantId",
                table: "WorkerType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Worker_TenantId",
                table: "Worker",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_User_TenantId",
                table: "User",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_TenantId",
                table: "Review",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_TenantId",
                table: "Reservation",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_TenantId",
                table: "Location",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Facility_TenantId",
                table: "Facility",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentType_TenantId",
                table: "AppointmentType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_TenantId",
                table: "Appointment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMember_TenantId",
                table: "AppMember",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppMember_Tenant_TenantId",
                table: "AppMember",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Tenant_TenantId",
                table: "Appointment",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentType_Tenant_TenantId",
                table: "AppointmentType",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Facility_Tenant_TenantId",
                table: "Facility",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Tenant_TenantId",
                table: "Location",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Tenant_TenantId",
                table: "Reservation",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Tenant_TenantId",
                table: "Review",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Tenant_TenantId",
                table: "User",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Worker_Tenant_TenantId",
                table: "Worker",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerType_Tenant_TenantId",
                table: "WorkerType",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppMember_Tenant_TenantId",
                table: "AppMember");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Tenant_TenantId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentType_Tenant_TenantId",
                table: "AppointmentType");

            migrationBuilder.DropForeignKey(
                name: "FK_Facility_Tenant_TenantId",
                table: "Facility");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_Tenant_TenantId",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Tenant_TenantId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Tenant_TenantId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Tenant_TenantId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_Worker_Tenant_TenantId",
                table: "Worker");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerType_Tenant_TenantId",
                table: "WorkerType");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropIndex(
                name: "IX_WorkerType_TenantId",
                table: "WorkerType");

            migrationBuilder.DropIndex(
                name: "IX_Worker_TenantId",
                table: "Worker");

            migrationBuilder.DropIndex(
                name: "IX_User_TenantId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Review_TenantId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_TenantId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Location_TenantId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Facility_TenantId",
                table: "Facility");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentType_TenantId",
                table: "AppointmentType");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_TenantId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_AppMember_TenantId",
                table: "AppMember");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WorkerType");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Worker");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Facility");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppointmentType");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppMember");
        }
    }
}
