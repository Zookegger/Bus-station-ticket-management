using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Vehicles_VehicleId",
                table: "Seats");

            migrationBuilder.RenameColumn(
                name: "VehicleId",
                table: "Seats",
                newName: "TripId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_VehicleId",
                table: "Seats",
                newName: "IX_Seats_TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Trips_TripId",
                table: "Seats",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Trips_TripId",
                table: "Seats");

            migrationBuilder.RenameColumn(
                name: "TripId",
                table: "Seats",
                newName: "VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_TripId",
                table: "Seats",
                newName: "IX_Seats_VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Vehicles_VehicleId",
                table: "Seats",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }
    }
}
