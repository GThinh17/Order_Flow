using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services
            .AddOptions<PulsarOptions>()
            .Bind(
                configuration.GetSection(
                    PulsarOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.ServiceUrl,
                    UriKind.Absolute,
                    out _),
                "Pulsar ServiceUrl is missing or invalid.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Topic),
                "Pulsar Topic is missing.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SubscriptionName),
                "Pulsar SubscriptionName is missing.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.DeadLetterTopic),
                "Pulsar DeadLetterTopic is missing.")
            .Validate(
                options => options.MaxDeliveryAttempts >= 1,
                "Pulsar MaxDeliveryAttempts must be at least 1.")
            .Validate(
                options => options.RedeliveryDelaySeconds >= 0,
                "Pulsar RedeliveryDelaySeconds cannot be negative.")
            .ValidateOnStart();

        services.AddSingleton<
            IEventPublisher,
            PulsarEventPublisher>();

        services.AddHostedService<
            OutboxPublisherWorker>();

        services.AddHostedService<
            OrdersSagaConsumerWorker>();

        return services;
    }
}
