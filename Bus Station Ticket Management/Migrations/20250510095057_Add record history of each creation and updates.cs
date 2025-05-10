using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class Addrecordhistoryofeachcreationandupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Routes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "VehicleTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "VehicleTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Trips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Trips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Routes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Routes");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "Routes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
