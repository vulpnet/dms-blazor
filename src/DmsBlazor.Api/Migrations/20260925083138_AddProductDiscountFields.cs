using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DmsBlazor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DiscountEligible",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraDiscountPercent",
                table: "products",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEligible",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ExtraDiscountPercent",
                table: "products");
        }
    }
}
