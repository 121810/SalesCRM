using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiristaCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportAndVehicleToStockMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FactoryToTransportCost",
                table: "StockMasters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrandTotalCost",
                table: "StockMasters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportToCompanyCost",
                table: "StockMasters",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VehicleNo",
                table: "StockMasters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "StockMasters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactoryToTransportCost",
                table: "StockMasters");

            migrationBuilder.DropColumn(
                name: "GrandTotalCost",
                table: "StockMasters");

            migrationBuilder.DropColumn(
                name: "TransportToCompanyCost",
                table: "StockMasters");

            migrationBuilder.DropColumn(
                name: "VehicleNo",
                table: "StockMasters");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "StockMasters");
        }
    }
}
