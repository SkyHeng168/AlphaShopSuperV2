using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaShop.Migrations
{
    /// <inheritdoc />
    public partial class ChangetoProfit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "Products",
                newName: "Profits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Profits",
                table: "Products",
                newName: "BasePrice");
        }
    }
}
