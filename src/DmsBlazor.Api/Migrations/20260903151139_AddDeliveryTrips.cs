using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DmsBlazor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.CreateSequence<int>(
                name: "trip_number_seq");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryFailureReason",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryStatus",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTripId",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "delivery_trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    DriverName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VehiclePlate = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DepartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VehiclePlate = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drivers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_DeliveryTripId",
                table: "orders",
                column: "DeliveryTripId");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_trips_TripCode",
                table: "delivery_trips",
                column: "TripCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_delivery_trips_DeliveryTripId",
                table: "orders",
                column: "DeliveryTripId",
                principalTable: "delivery_trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_delivery_trips_DeliveryTripId",
                table: "orders");

            migrationBuilder.DropTable(
                name: "delivery_trips");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropIndex(
                name: "IX_orders_DeliveryTripId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryFailureReason",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryTripId",
                table: "orders");

            migrationBuilder.DropSequence(
                name: "trip_number_seq");

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DistanceKm = table.Column<int>(type: "integer", nullable: false),
                    Distributor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EtaHours = table.Column<double>(type: "double precision", nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Vehicle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timeline = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shipments_Code",
                table: "shipments",
                column: "Code",
                unique: true);
        }
    }
}
