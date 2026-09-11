using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Infrastructure.Health;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseConnectionString =
            configuration["Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(databaseConnectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured.");
        }

        var pulsarAdminUrl = configuration["Pulsar:AdminUrl"];

        if (!Uri.TryCreate(
                pulsarAdminUrl,
                UriKind.Absolute,
                out var pulsarAdminUri))
        {
            throw new InvalidOperationException(
                "Pulsar admin URL is missing or invalid.");
        }

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(databaseConnectionString));

        services.AddHttpClient(
            PulsarHealthCheck.HttpClientName,
            client =>
            {
                client.BaseAddress = pulsarAdminUri;
                client.Timeout = TimeSpan.FromSeconds(3);
            });

        services
            .AddHealthChecks()
            .AddDbContextCheck<PaymentsDbContext>("database")
            .AddCheck<PulsarHealthCheck>("pulsar");

        return services;
    }
}
