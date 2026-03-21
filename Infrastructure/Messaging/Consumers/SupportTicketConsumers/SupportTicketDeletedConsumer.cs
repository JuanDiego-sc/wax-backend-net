using Application.IntegrationEvents.SupportTicketEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketDeletedConsumer(ReadDbContext readContext, ILogger<SupportTicketDeletedConsumer> logger)
    : IConsumer<SupportTicketDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SupportTicketDeletedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == message.TicketId, context.CancellationToken);

        if (readModel is null)
        {
            logger.LogWarning("SupportTicket read model not found for deletion: TicketId={TicketId}", message.TicketId);
            return;
        }

        readContext.SupportTickets.Remove(readModel);
        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("SupportTicket with id {TicketId} has been deleted", message.TicketId);
    }
}
