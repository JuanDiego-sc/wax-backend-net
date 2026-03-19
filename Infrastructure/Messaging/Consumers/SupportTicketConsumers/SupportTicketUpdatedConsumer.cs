using Application.IntegrationEvents.SupportTicketEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketUpdatedConsumer(ReadDbContext readContext, ILogger<SupportTicketUpdatedConsumer> logger)
    : IConsumer<SupportTicketUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SupportTicketUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == message.TicketId, context.CancellationToken);

        if (readModel is null)
        {
            logger.LogWarning("SupportTicket read model not found: TicketId={TicketId}", message.TicketId);
            throw new InvalidOperationException($"SupportTicket read model not found: {message.TicketId}");
        }

        readModel.Category = message.Category;
        readModel.Status = message.Status;
        readModel.Subject = message.Subject;
        readModel.Description = message.Description;
        readModel.UpdatedAt = message.OccurredAt;
        readModel.LastSyncedAt = DateTime.UtcNow;

        readContext.Entry(readModel).State = EntityState.Modified;
        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("SupportTicket with id {TicketId} has been updated", message.TicketId);
    }
}
