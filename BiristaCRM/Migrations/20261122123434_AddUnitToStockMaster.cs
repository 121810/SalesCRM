using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiristaCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToStockMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "StockMasters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "StockMasters");
        }
    }
}
