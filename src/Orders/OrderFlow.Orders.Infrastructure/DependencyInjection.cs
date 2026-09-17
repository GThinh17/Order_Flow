using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Messaging;
using OrderFlow.Orders.Application.Abstractions.Persistence;
using OrderFlow.Orders.Infrastructure.Health;
using OrderFlow.Orders.Infrastructure.Messaging;
using OrderFlow.Orders.Infrastructure.Persistence;
using OrderFlow.Orders.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Orders.Infrastructure;

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

        services.AddDbContext<OrdersDbContext>(
            options => options.UseNpgsql(
                databaseConnectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable(
                        "__ef_migrations_history",
                        OrdersDbContext.SchemaName);

                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorCodesToAdd:
                        [
                            "40001",
                            "40P01"
                        ]);
                }));

        services.AddScoped<
            IOutboxWriter,
            OutboxWriter>();

        services.AddScoped<
            IOrderRepository,
            OrderRepository>();

        services.AddScoped<
            IOrderSagaStateRepository,
            OrderSagaStateRepository>();

        services.AddScoped<
            IInboxRepository,
            InboxRepository>();

        services.AddScoped<
            IOrdersTransactionRunner,
            EfOrdersTransactionRunner>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OrdersDbContext>());

        services.AddHttpClient(
            PulsarHealthCheck.HttpClientName,
            client =>
            {
                client.BaseAddress = pulsarAdminUri;
                client.Timeout = TimeSpan.FromSeconds(3);
            });

        services
            .AddHealthChecks()
            .AddDbContextCheck<OrdersDbContext>("database")
            .AddCheck<PulsarHealthCheck>("pulsar");

        services.AddPulsarOptions(configuration);

        services.AddSingleton<
            IEventPublisher,
            PulsarEventPublisher>();

        services.AddHostedService<
            OrdersPublisher>();

        services.AddHostedService<
            OrdersConsumer>();

        services.AddSingleton<
            IOrdersEventDispatcher,
            OrdersEventDispatcher>();

        services.AddSingleton<
            PulsarFailedMessageHandler>();

        return services;
    }
}
