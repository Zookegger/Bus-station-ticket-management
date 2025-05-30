using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class VehicleTypeUpdatetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRows",
                table: "VehicleTypes");

            migrationBuilder.AddColumn<string>(
                name: "RowsPerFloor",
                table: "VehicleTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowsPerFloor",
                table: "VehicleTypes");

            migrationBuilder.AddColumn<int>(
                name: "TotalRows",
                table: "VehicleTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
