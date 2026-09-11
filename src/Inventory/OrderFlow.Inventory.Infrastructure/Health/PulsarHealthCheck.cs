using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlow.Inventory.Infrastructure.Health;

public sealed class PulsarHealthCheck(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
    : IHealthCheck
{
    public const string HttpClientName = "PulsarHealth";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var lookupPath = configuration["Pulsar:LookupPath"];

        if (string.IsNullOrWhiteSpace(lookupPath))
        {
            return HealthCheckResult.Unhealthy(
                "Pulsar lookup path is not configured.");
        }

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            using var response = await client.GetAsync(
                lookupPath,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy(
                $"Pulsar lookup returned HTTP {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy(
                "Pulsar lookup timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            return HealthCheckResult.Unhealthy(
                "Pulsar is unreachable.",
                exception);
        }
    }
}
