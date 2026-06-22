using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Migrations
{
    public partial class FixCategoryColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ShopCategories_ShopCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ShopCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShopCategoryId",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShopCategoryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShopCategoryId",
                table: "Products",
                column: "ShopCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ShopCategories_ShopCategoryId",
                table: "Products",
                column: "ShopCategoryId",
                principalTable: "ShopCategories",
                principalColumn: "Id");
        }
    }
}
