using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Messaging;

public sealed class PaymentsPublisher
    : BackgroundService
{
    private const int BatchSize = 20;

    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPaymentEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PaymentsPublisher> _logger;

    public PaymentsPublisher(
        IServiceScopeFactory scopeFactory,
        IPaymentEventPublisher eventPublisher,
        TimeProvider timeProvider,
        ILogger<PaymentsPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _eventPublisher = eventPublisher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to publish Payments outbox messages.");
            }

            try
            {
                await Task.Delay(
                    PollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task PublishBatchAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<PaymentsDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            await _eventPublisher.PublishAsync(
                message.EventId,
                message.EventType,
                message.PartitionKey,
                message.Payload,
                cancellationToken);

            message.MarkAsPublished(
                _timeProvider.GetUtcNow());

            await dbContext.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                "Published Payments outbox event {EventId} of type {EventType}.",
                message.EventId,
                message.EventType);
        }
    }
}
