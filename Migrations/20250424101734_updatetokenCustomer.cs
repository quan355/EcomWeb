using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComWeb.Migrations
{
    /// <inheritdoc />
    public partial class updatetokenCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "tbl_customer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordTokenExpiry",
                table: "tbl_customer",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_cust_id",
                table: "tbl_cart",
                column: "cust_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_cart_product_id",
                table: "tbl_cart",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_cart_tbl_customer_cust_id",
                table: "tbl_cart",
                column: "cust_id",
                principalTable: "tbl_customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_cart_tbl_product_product_id",
                table: "tbl_cart",
                column: "product_id",
                principalTable: "tbl_product",
                principalColumn: "products_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cart_tbl_customer_cust_id",
                table: "tbl_cart");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_cart_tbl_product_product_id",
                table: "tbl_cart");

            migrationBuilder.DropIndex(
                name: "IX_tbl_cart_cust_id",
                table: "tbl_cart");

            migrationBuilder.DropIndex(
                name: "IX_tbl_cart_product_id",
                table: "tbl_cart");

            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "tbl_customer");

            migrationBuilder.DropColumn(
                name: "ResetPasswordTokenExpiry",
                table: "tbl_customer");
        }
    }
}
