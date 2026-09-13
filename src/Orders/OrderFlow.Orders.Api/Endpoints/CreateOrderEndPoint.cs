using OrderFlow.Orders.Api.Contract.Orders;
using OrderFlow.Orders.Application.OrderCommand;

namespace OrderFlow.Orders.Api.Endpoints.Orders;

public static class CreateOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/orders", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateOrderRequest request,
        CreateOrderHandler handler,
        CancellationToken cancellationToken)
    {
        if (request.Lines is null)
        {
            return Results.BadRequest(
                new { error = "Lines are required." });
        }

        var command = new CreateOrderCommand(
            request.CustomerId,
            request.Lines
                .Select(line =>
                    new CreateOrderLineCommand(
                        line.Sku,
                        line.Quantity,
                        line.UnitPrice))
                .ToArray());

        try
        {
            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            var response = new CreateOrderResponse(
                result.OrderId,
                result.CorrelationId,
                result.Status.ToString());

            return Results.Accepted(
                $"/orders/{result.OrderId}",
                response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(
                new { error = exception.Message });
        }
    }
}
