using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DmsBlazor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistributorId",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceWarehouseId",
                table: "delivery_trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceWarehouseName",
                table: "delivery_trips",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "inventory_stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_stocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    QuantityChange = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    DistributorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stocks_WarehouseId_ProductId",
                table: "inventory_stocks",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_DistributorId",
                table: "warehouses",
                column: "DistributorId",
                unique: true,
                filter: "\"DistributorId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_stocks");

            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "warehouses");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseId",
                table: "delivery_trips");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseName",
                table: "delivery_trips");
        }
    }
}
