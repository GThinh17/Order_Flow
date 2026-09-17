using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Health;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Infrastructure.Messaging;
using OrderFlow.Inventory.Infrastructure.Persistence.Repositories;
using OrderFlow.Messaging;

namespace OrderFlow.Inventory.Infrastructure;

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

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                databaseConnectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable(
                        "__ef_migrations_history",
                        InventoryDbContext.SchemaName);

                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorCodesToAdd:
                        [
                            "40001",
                            "40P01"
                        ]);
                }));

        services.AddHttpClient(
            PulsarHealthCheck.HttpClientName,
            client =>
            {
                client.BaseAddress = pulsarAdminUri;
                client.Timeout = TimeSpan.FromSeconds(3);
            });

        services
            .AddHealthChecks()
            .AddDbContextCheck<InventoryDbContext>("database")
            .AddCheck<PulsarHealthCheck>("pulsar");

        services.AddScoped<
            IStockRepository,
            StockRepository>();

        services.AddScoped<
            IInventoryTransactionRunner,
            EfInventoryTransactionRunner>();

        services.AddScoped<
            IInboxRepository,
            InboxRepository>();

        services.AddScoped<
            IReservationRepository,
            ReservationRepository>();

        services.AddScoped<
            IOutboxWriter,
            InventoryOutboxWriter>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    InventoryDbContext>());

        services.AddPulsarOptions(configuration);

        services.AddHostedService<InventoryConsumer>();

        services.AddSingleton<
            IInventoryEventPublisher,
            PulsarInventoryEventPublisher>();

        services.AddSingleton<
            IInventoryEventDispatcher,
            InventoryEventDispatcher>();

        services.AddHostedService<
            InventoryPublisher>();

        services.AddSingleton<
            PulsarFailedMessageHandler>();

        return services;
    }
}
