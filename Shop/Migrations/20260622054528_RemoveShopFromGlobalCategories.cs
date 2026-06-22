using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShopFromGlobalCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopCategories_Shops_ShopId",
                table: "ShopCategories");

            migrationBuilder.DropIndex(
                name: "IX_ShopCategories_ShopId",
                table: "ShopCategories");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "ShopCategories");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "ShopCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCategories_ShopId",
                table: "ShopCategories",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopCategories_Shops_ShopId",
                table: "ShopCategories",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
