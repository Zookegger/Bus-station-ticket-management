using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class VehicleTypeFixgrammars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalRow",
                table: "VehicleTypes",
                newName: "TotalRows");

            migrationBuilder.RenameColumn(
                name: "TotalColumn",
                table: "VehicleTypes",
                newName: "TotalColumns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalRows",
                table: "VehicleTypes",
                newName: "TotalRow");

            migrationBuilder.RenameColumn(
                name: "TotalColumns",
                table: "VehicleTypes",
                newName: "TotalColumn");
        }
    }
}
