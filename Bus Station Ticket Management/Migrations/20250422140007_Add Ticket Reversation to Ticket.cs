using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketReversationtoTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_VnPayments_VnPaymentTransactionNo",
                table: "Payments");

            migrationBuilder.AddColumn<bool>(
                name: "IsReserved",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_VnPayments_VnPaymentTransactionNo",
                table: "Payments",
                column: "VnPaymentTransactionNo",
                principalTable: "VnPayments",
                principalColumn: "TransactionNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_VnPayments_VnPaymentTransactionNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsReserved",
                table: "Tickets");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_VnPayments_VnPaymentTransactionNo",
                table: "Payments",
                column: "VnPaymentTransactionNo",
                principalTable: "VnPayments",
                principalColumn: "TransactionNo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
