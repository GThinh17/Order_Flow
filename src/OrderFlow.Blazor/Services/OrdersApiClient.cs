using System.Net;
using System.Net.Http.Json;
using OrderFlow.Blazor.Models.Orders;

namespace OrderFlow.Blazor.Services;

public sealed class OrdersApiClient
{
    private readonly HttpClient _httpClient;

    public OrdersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "/orders",
            request,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<CreateOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException(
                "Orders API returned an empty create-order response.");
    }

    public async Task<GetOrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"/orders/{orderId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<GetOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException(
                "Orders API returned an empty order response.");
    }

    public async Task<IReadOnlyCollection<OrderSummaryResponse>>
        GetOrdersByCustomerAsync(
            string customerId,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));
        }

        var encodedCustomerId = Uri.EscapeDataString(
            customerId.Trim());

        using var response = await _httpClient.GetAsync(
            $"/orders?customerId={encodedCustomerId}",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<OrderSummaryResponse[]>(cancellationToken)
            ?? [];
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content
            .ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Orders API returned {(int)response.StatusCode} " +
            $"({response.ReasonPhrase}). {responseBody}",
            inner: null,
            response.StatusCode);
    }
}
