using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPhoneColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_phone",
                table: "tbl_customer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_phone",
                table: "tbl_customer");
        }
    }
}
