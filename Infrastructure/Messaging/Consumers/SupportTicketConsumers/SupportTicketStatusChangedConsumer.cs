using Application.IntegrationEvents.SupportTicketEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketStatusChangedConsumer(ReadDbContext readContext, ILogger<SupportTicketStatusChangedConsumer> logger)
    : IConsumer<SupportTicketStatusChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SupportTicketStatusChangedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == message.TicketId, context.CancellationToken);

        if (readModel is null)
        {
            logger.LogWarning("SupportTicket read model not found: TicketId={TicketId}", message.TicketId);
            throw new InvalidOperationException($"SupportTicket read model not found: {message.TicketId}");
        }

        readModel.Status = message.NewStatus;
        readModel.UpdatedAt = message.OccurredAt;
        readModel.LastSyncedAt = DateTime.UtcNow;

        readContext.Entry(readModel).State = EntityState.Modified;
        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("SupportTicket status changed: TicketId={TicketId}, NewStatus={NewStatus}", 
            message.TicketId, message.NewStatus);
    }
}
