
using OrderFlow.Inventory.Api.Contracts;
using OrderFlow.Inventory.Application.InventoryCommand;
using OrderFlow.Inventory.Application.Stock;

namespace OrderFlow.Inventory.Api.Endpoints
{
    public static class StockEndpoints
    {
        public static IEndpointRouteBuilder MapStockEndpoints(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(
                "/stock",
                GetStockAsync);

            endpoints.MapPost(
                "/stock/{sku}/adjust",
                AdjustStockAsync);

            return endpoints;
        }

        private static async Task<IResult> GetStockAsync(
            GetStockHandler handler,
            CancellationToken cancellationToken)
        {
            var results =
                await handler.HandleAsync(
                    cancellationToken);

            var response = results
                .Select(stock => new StockResponse(
                    stock.Sku,
                    stock.QuantityOnHand,
                    stock.QuantityReserved,
                    stock.Available))
                .ToArray();

            return Results.Ok(response);
        }

        private static async Task<IResult> AdjustStockAsync(
            string sku,
            AdjustStockRequest request,
            AdjustStockHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new AdjustStockCommand(
                    sku,
                    request.Quantity);

                var result = await handler.HandleAsync(
                    command,
                    cancellationToken);

                if (result is null)
                {
                    return Results.NotFound(
                        new { error = $"SKU '{sku}' was not found." });
                }

                return Results.Ok(
                    new StockResponse(
                        result.Sku,
                        result.QuantityOnHand,
                        result.QuantityReserved,
                        result.Available));
            }
            catch (Exception exception)
                when (exception is ArgumentException
                    or InvalidOperationException
                    or OverflowException)
            {
                return Results.BadRequest(
                    new { error = exception.Message });
            }
        }
    }
}

