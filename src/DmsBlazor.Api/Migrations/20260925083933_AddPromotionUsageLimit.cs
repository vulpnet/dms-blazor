using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DmsBlazor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionUsageLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxUsagePerDistributorPerMonth",
                table: "promotion_rules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "promotion_rule_usages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleId = table.Column<int>(type: "integer", nullable: false),
                    DistributorId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_rule_usages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_promotion_rule_usages_RuleId_DistributorId_Year_Month",
                table: "promotion_rule_usages",
                columns: new[] { "RuleId", "DistributorId", "Year", "Month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promotion_rule_usages");

            migrationBuilder.DropColumn(
                name: "MaxUsagePerDistributorPerMonth",
                table: "promotion_rules");
        }
    }
}
