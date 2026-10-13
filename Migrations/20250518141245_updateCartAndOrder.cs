using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComWeb.Migrations
{
    /// <inheritdoc />
    public partial class updateCartAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "product_size",
                table: "tbl_order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "product_size",
                table: "tbl_cart",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_size",
                table: "tbl_order");

            migrationBuilder.DropColumn(
                name: "product_size",
                table: "tbl_cart");
        }
    }
}
