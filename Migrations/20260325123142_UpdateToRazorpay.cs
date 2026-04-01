using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryEnterpriseProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToRazorpay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripePaymentIntentId",
                table: "Payments",
                newName: "RazorpaySignature");

            migrationBuilder.RenameColumn(
                name: "StripeClientSecret",
                table: "Payments",
                newName: "RazorpayPaymentId");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "Payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "RazorpaySignature",
                table: "Payments",
                newName: "StripePaymentIntentId");

            migrationBuilder.RenameColumn(
                name: "RazorpayPaymentId",
                table: "Payments",
                newName: "StripeClientSecret");
        }
    }
}
