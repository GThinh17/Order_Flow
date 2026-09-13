using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure.Messaging
{
    public sealed class OutboxPublisherWorker
        : BackgroundService
    {
        private const int BatchSize = 20;

        private static readonly TimeSpan PollingInterval =
            TimeSpan.FromSeconds(2);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            IEventPublisher eventPublisher,
            TimeProvider timeProvider,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _eventPublisher = eventPublisher;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Failed to publish Orders outbox messages");
                }
                try
                {
                    await Task.Delay(
                        PollingInterval,
                        stoppingToken);
                }
                catch (System.Exception)
                {

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
        }
        private async Task PublishBatchAsync(
            CancellationToken cancellationToken)
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<OrdersDbContext>();

            var messages = await dbContext
                .OutboxMessages
                .Where(message =>
                    message.PublishedAt == null)
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
                    "Published outbox event {EventId} of type {EventType}.",
                    message.EventId,
                    message.EventType);
            }
        }
    }
}
