using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRouteStartIdandDestIdtocorrectforeignkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_DestinationLocation",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_StartLocation",
                table: "Routes");

            migrationBuilder.RenameColumn(
                name: "StartLocation",
                table: "Routes",
                newName: "StartId");

            migrationBuilder.RenameColumn(
                name: "DestinationLocation",
                table: "Routes",
                newName: "DestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_StartLocation",
                table: "Routes",
                newName: "IX_Routes_StartId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DestinationLocation",
                table: "Routes",
                newName: "IX_Routes_DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_DestinationId",
                table: "Routes",
                column: "DestinationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_StartId",
                table: "Routes",
                column: "StartId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_DestinationId",
                table: "Routes");

            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Locations_StartId",
                table: "Routes");

            migrationBuilder.RenameColumn(
                name: "StartId",
                table: "Routes",
                newName: "StartLocation");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "Routes",
                newName: "DestinationLocation");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_StartId",
                table: "Routes",
                newName: "IX_Routes_StartLocation");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_DestinationId",
                table: "Routes",
                newName: "IX_Routes_DestinationLocation");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_DestinationLocation",
                table: "Routes",
                column: "DestinationLocation",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Locations_StartLocation",
                table: "Routes",
                column: "StartLocation",
                principalTable: "Locations",
                principalColumn: "Id");
        }
    }
}
