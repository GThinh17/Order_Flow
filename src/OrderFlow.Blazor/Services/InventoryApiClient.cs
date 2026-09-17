using System.Net.Http.Json;
using OrderFlow.Blazor.Models.Inventory;

namespace OrderFlow.Blazor.Services;

public sealed class InventoryApiClient
{
    private readonly HttpClient _httpClient;

    public InventoryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<StockResponse>> GetStockAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "/stock",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content
                .ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Inventory API returned {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}). {responseBody}",
                inner: null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<StockResponse[]>(cancellationToken)
            ?? [];
    }
}
