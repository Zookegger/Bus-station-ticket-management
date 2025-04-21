using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bus_Station_Ticket_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentColumnstoTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tickets_TicketId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TicketId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "TotalAmount");

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnPaymentTransactionNo",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VnPaymentTransactionNo",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VnPayments",
                columns: table => new
                {
                    TransactionNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankTranNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayDate = table.Column<long>(type: "bigint", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TmnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TxnRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecureHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnPayments", x => x.TransactionNo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VnPaymentTransactionNo",
                table: "Tickets",
                column: "VnPaymentTransactionNo");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VnPaymentTransactionNo",
                table: "Payments",
                column: "VnPaymentTransactionNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_VnPayments_VnPaymentTransactionNo",
                table: "Payments",
                column: "VnPaymentTransactionNo",
                principalTable: "VnPayments",
                principalColumn: "TransactionNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_VnPayments_VnPaymentTransactionNo",
                table: "Tickets",
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

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_VnPayments_VnPaymentTransactionNo",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "VnPayments");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_VnPaymentTransactionNo",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_VnPaymentTransactionNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VnPaymentTransactionNo",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VnPaymentTransactionNo",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Payments",
                newName: "Amount");

            migrationBuilder.AddColumn<string>(
                name: "TicketId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TicketId",
                table: "Payments",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tickets_TicketId",
                table: "Payments",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
