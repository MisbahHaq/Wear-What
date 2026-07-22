using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiVendor.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddShopProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileBannerUrl",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileBannerUrl",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Shops");
        }
    }
}

