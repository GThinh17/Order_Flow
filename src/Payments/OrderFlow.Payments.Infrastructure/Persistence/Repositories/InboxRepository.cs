using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Entity;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Persistence.Repositories
{
    public sealed class InboxRepository : IInboxRepository
    {
        private readonly PaymentsDbContext _paymentDbContext;

        public InboxRepository(PaymentsDbContext paymentsDbContext)
        {
            _paymentDbContext = paymentsDbContext;
        }

        public Task<bool> ExistAsync(
                Guid eventId,
                CancellationToken cancellationToken = default)
        {
            return _paymentDbContext.InboxMessages
                        .AsNoTracking()
                        .AnyAsync(
                            message => message.EventId == eventId,
                            cancellationToken);
        }
        public void Add(
            Guid eventId,
            string eventType,
            DateTimeOffset processedAt)
        {
            _paymentDbContext.InboxMessages.Add(
                new InboxMessage(
                    eventId,
                    eventType,
                    processedAt));
        }
    }
}