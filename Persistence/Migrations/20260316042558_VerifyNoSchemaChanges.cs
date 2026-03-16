using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VerifyNoSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_State",
                table: "Orders",
                newName: "Billing_State");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_PostalCode",
                table: "Orders",
                newName: "Billing_PostalCode");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Name",
                table: "Orders",
                newName: "Billing_Address");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Line2",
                table: "Orders",
                newName: "Billing_Line2");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Line1",
                table: "Orders",
                newName: "Billing_Line1");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_Country",
                table: "Orders",
                newName: "Billing_Country");

            migrationBuilder.RenameColumn(
                name: "ShippingAddress_City",
                table: "Orders",
                newName: "Billing_City");

            migrationBuilder.RenameColumn(
                name: "PaymentSummary_Last4",
                table: "Orders",
                newName: "Payment_Last4");

            migrationBuilder.RenameColumn(
                name: "PaymentSummary_ExpYear",
                table: "Orders",
                newName: "Payment_ExpYear");

            migrationBuilder.RenameColumn(
                name: "PaymentSummary_ExpMonth",
                table: "Orders",
                newName: "Payment_ExpMonth");

            migrationBuilder.RenameColumn(
                name: "PaymentSummary_Brand",
                table: "Orders",
                newName: "Payment_Brand");

            migrationBuilder.RenameColumn(
                name: "ItemOrdered_ProductId",
                table: "OrderItems",
                newName: "Item_ProductId");

            migrationBuilder.RenameColumn(
                name: "ItemOrdered_Name",
                table: "OrderItems",
                newName: "Item_ProductName");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SupportTickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_BasketId",
                table: "Baskets",
                column: "BasketId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Baskets_BasketId",
                table: "Baskets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "Payment_Last4",
                table: "Orders",
                newName: "PaymentSummary_Last4");

            migrationBuilder.RenameColumn(
                name: "Payment_ExpYear",
                table: "Orders",
                newName: "PaymentSummary_ExpYear");

            migrationBuilder.RenameColumn(
                name: "Payment_ExpMonth",
                table: "Orders",
                newName: "PaymentSummary_ExpMonth");

            migrationBuilder.RenameColumn(
                name: "Payment_Brand",
                table: "Orders",
                newName: "PaymentSummary_Brand");

            migrationBuilder.RenameColumn(
                name: "Billing_State",
                table: "Orders",
                newName: "ShippingAddress_State");

            migrationBuilder.RenameColumn(
                name: "Billing_PostalCode",
                table: "Orders",
                newName: "ShippingAddress_PostalCode");

            migrationBuilder.RenameColumn(
                name: "Billing_Line2",
                table: "Orders",
                newName: "ShippingAddress_Line2");

            migrationBuilder.RenameColumn(
                name: "Billing_Line1",
                table: "Orders",
                newName: "ShippingAddress_Line1");

            migrationBuilder.RenameColumn(
                name: "Billing_Country",
                table: "Orders",
                newName: "ShippingAddress_Country");

            migrationBuilder.RenameColumn(
                name: "Billing_City",
                table: "Orders",
                newName: "ShippingAddress_City");

            migrationBuilder.RenameColumn(
                name: "Billing_Address",
                table: "Orders",
                newName: "ShippingAddress_Name");

            migrationBuilder.RenameColumn(
                name: "Item_ProductName",
                table: "OrderItems",
                newName: "ItemOrdered_Name");

            migrationBuilder.RenameColumn(
                name: "Item_ProductId",
                table: "OrderItems",
                newName: "ItemOrdered_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
