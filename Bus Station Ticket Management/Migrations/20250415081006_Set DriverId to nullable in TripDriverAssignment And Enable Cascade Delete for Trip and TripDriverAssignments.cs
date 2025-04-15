using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class SetDriverIdtonullableinTripDriverAssignmentAndEnableCascadeDeleteforTripandTripDriverAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripDriverAssignments_Drivers_DriverId",
                table: "TripDriverAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TripDriverAssignments_Trips_TripId",
                table: "TripDriverAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "TripDriverAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TripDriverAssignments_Drivers_DriverId",
                table: "TripDriverAssignments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TripDriverAssignments_Trips_TripId",
                table: "TripDriverAssignments",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripDriverAssignments_Drivers_DriverId",
                table: "TripDriverAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TripDriverAssignments_Trips_TripId",
                table: "TripDriverAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "TripDriverAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TripDriverAssignments_Drivers_DriverId",
                table: "TripDriverAssignments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripDriverAssignments_Trips_TripId",
                table: "TripDriverAssignments",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id");
        }
    }
}
