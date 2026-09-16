using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DmsBlazor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockOverCreditLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BlockOverCreditLimit",
                table: "distributors",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockOverCreditLimit",
                table: "distributors");
        }
    }
}
