using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class DriverExpandonDriversLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "DriverLicenses",
                columns: table => new
                {
                    DriverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicenseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicenseClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseIssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LicenseExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LicenseIssuePlace = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverLicenses", x => new { x.DriverId, x.LicenseId });
                    table.ForeignKey(
                        name: "FK_DriverLicenses_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverLicenses");

            migrationBuilder.AddColumn<string>(
                name: "LicenseId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
