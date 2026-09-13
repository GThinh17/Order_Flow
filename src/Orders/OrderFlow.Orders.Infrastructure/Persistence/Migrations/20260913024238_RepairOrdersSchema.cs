using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrdersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "partitionkey",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "partition_key");

            migrationBuilder.RenameColumn(
                name: "EventId",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "event_id");

            migrationBuilder.RenameColumn(
                name: "create_at",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "publish_at",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "update_at",
                schema: "orderflow_orders",
                table: "orders",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "create_at",
                schema: "orderflow_orders",
                table: "orders",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Sku",
                schema: "orderflow_orders",
                table: "order_lines",
                newName: "sku");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "published_at",
                schema: "orderflow_orders",
                table: "out_messages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "quantity",
                schema: "orderflow_orders",
                table: "order_lines",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "sku",
                schema: "orderflow_orders",
                table: "order_lines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE orderflow_orders.out_messages
                SET published_at = created_at
                WHERE published_at IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "published_at",
                schema: "orderflow_orders",
                table: "out_messages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "partition_key",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "partitionkey");

            migrationBuilder.RenameColumn(
                name: "event_id",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "create_at");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "orderflow_orders",
                table: "out_messages",
                newName: "publish_at");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "orderflow_orders",
                table: "orders",
                newName: "update_at");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "orderflow_orders",
                table: "orders",
                newName: "create_at");

            migrationBuilder.RenameColumn(
                name: "sku",
                schema: "orderflow_orders",
                table: "order_lines",
                newName: "Sku");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                schema: "orderflow_orders",
                table: "order_lines",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity",
                schema: "orderflow_orders",
                table: "order_lines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
