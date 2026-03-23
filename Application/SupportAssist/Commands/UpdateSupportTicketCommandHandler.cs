using Application.Core;
using Application.IntegrationEvents.SupportTicketEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.SupportAssist.Commands;

public class UpdateSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSupportTicketCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetTicketByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null) return Result<Unit>.Failure("Support ticket not found");

        var previousStatus = ticket.Status;
        
        request.TicketDto.ApplyTo(ticket);

        await eventPublisher.PublishEventAsync(new SupportTicketUpdatedIntegrationEvent
        {
            TicketId = ticket.Id,
            Category = ticket.Category.ToString(),
            Status = ticket.Status.ToString(),
            Subject = ticket.Subject,
            Description = ticket.Description
        }, cancellationToken);

        if (previousStatus != ticket.Status)
        {
            await eventPublisher.PublishEventAsync(new SupportTicketStatusChangedIntegrationEvent
            {
                TicketId = ticket.Id,
                NewStatus = ticket.Status.ToString()
            }, cancellationToken);
        }

        var result = await unitOfWork.CompleteAsync(cancellationToken);

        return !result
            ? Result<Unit>.Failure("An error occurred saving the data")
            : Result<Unit>.Success(Unit.Value);
    }
}
