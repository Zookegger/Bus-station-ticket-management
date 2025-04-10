using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalTotalPricetoTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalPrice",
                table: "Tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Tickets");
        }
    }
}
