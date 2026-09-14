using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersSagaAndInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "orderflow_orders",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "order_saga_state",
                schema: "orderflow_orders",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reservation_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    payment_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_processed_event_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_saga_state", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_order_saga_state_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "orderflow_orders",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO orderflow_orders.order_saga_state
                    (order_id, reservation_completed, payment_completed, last_processed_event_id)
                SELECT
                    id, FALSE, FALSE, NULL
                FROM orderflow_orders.orders
                ON CONFLICT (order_id) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "orderflow_orders");

            migrationBuilder.DropTable(
                name: "order_saga_state",
                schema: "orderflow_orders");
        }
    }
}
