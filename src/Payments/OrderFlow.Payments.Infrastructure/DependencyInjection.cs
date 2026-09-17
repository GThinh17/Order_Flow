using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Application.Handler;
using OrderFlow.Payments.Infrastructure.Health;
using OrderFlow.Payments.Infrastructure.PaymentGateWay;
using OrderFlow.Payments.Infrastructure.Persistence;
using OrderFlow.Payments.Infrastructure.Messaging;
using OrderFlow.Payments.Infrastructure.Persistence.Repositories;
using OrderFlow.Messaging;

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
            options.UseNpgsql(
                databaseConnectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable(
                        "__ef_migrations_history",
                        PaymentsDbContext.SchemaName);

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

        services.AddScoped<
            IPaymentRepository,
            PaymentRepository>();

        services.AddScoped<
            IInboxRepository,
            InboxRepository>();

        services.AddScoped<
            IOutboxWriter,
            OutboxWriter>();

        services.AddScoped<
            IPaymentTransactionRunner,
            EfPaymentsTransactionRunner>();

        services.AddSingleton<TimeProvider>(
            TimeProvider.System);

        services.AddSingleton<
            IPaymentGateWay,
            PaymentGateway>();

        services.AddScoped<
            ProcessReservationSucceededHandler>();

        services.AddPulsarOptions(configuration);

        services.AddHostedService<
            PaymentsConsumer>();

        services.AddSingleton<
            IPaymentEventPublisher,
            PulsarPaymentEventPublisher>();

        services.AddHostedService<
            PaymentsPublisher>();

        services.AddSingleton<
            IPaymentsEventDispatcher,
            PaymentsEventDispatcher>();

        services.AddSingleton<
            PulsarFailedMessageHandler>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<PaymentsDbContext>("database")
            .AddCheck<PulsarHealthCheck>("pulsar");

        return services;
    }
}
