using Application.Core;
using Application.Core.Validations;
using Application.IntegrationEvents.SupportTicketEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.SupportAssist.Commands;

public class DeleteSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSupportTicketCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetTicketByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null) return Result<Unit>.Failure("Support ticket not found");

        supportTicketRepository.Remove(ticket);

        await eventPublisher.PublishEventAsync(new SupportTicketDeletedIntegrationEvent
        {
            TicketId = ticket.Id
        }, cancellationToken);

        var result = await unitOfWork.CompleteAsync(cancellationToken);

        return !result
            ? Result<Unit>.Failure("An error occurred deleting the data")
            : Result<Unit>.Success(Unit.Value);
    }
}
