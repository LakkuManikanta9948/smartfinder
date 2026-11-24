using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartDealFinder.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMatchingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gtin",
                table: "products",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "products",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Variant",
                table: "products",
                type: "varchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gtin",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Variant",
                table: "products");
        }
    }
}
