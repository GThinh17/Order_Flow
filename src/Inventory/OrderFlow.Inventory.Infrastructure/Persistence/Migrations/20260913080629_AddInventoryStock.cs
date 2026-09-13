using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orderflow_inventory");

            migrationBuilder.CreateTable(
                name: "stock_items",
                schema: "orderflow_inventory",
                columns: table => new
                {
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity_on_hand = table.Column<int>(type: "integer", nullable: false),
                    quantity_reserved = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_items", x => x.sku);
                    table.CheckConstraint("ck_stock_on_hand_non_nagative", "quantity_on_hand >= 0");
                    table.CheckConstraint("ck_stock_reserved_non_nagative", "quantity_reserved >= 0");
                    table.CheckConstraint("ck_stock_reserved_not_exceed_on_hanh", "quantity_reserved <= quantity_on_hand");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_items",
                schema: "orderflow_inventory");
        }
    }
}
