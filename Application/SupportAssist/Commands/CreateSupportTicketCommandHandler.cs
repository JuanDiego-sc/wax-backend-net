using Application.Core;
using Application.Core.Validations;
using Application.IntegrationEvents.SupportTicketEvents;
using Application.Interfaces.Services;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.SupportAssist.Commands;

public class CreateSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    IOrderRepository orderRepository,
    IUserAccessor userAccessor,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSupportTicketCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository
            .GetByOrderIdAsync(request.TicketDto.OrderId, cancellationToken);

        if (order == null) return Result<string>.Failure("Order not found");

        var user = await userAccessor.GetUserAsync();

        if (user == null) return Result<string>.Failure("User not found");
        
        var ticket = request.TicketDto.ToEntity(user.Id);
        
        supportTicketRepository.Add(ticket);

        await eventPublisher.PublishEventAsync(new SupportTicketCreatedIntegrationEvent
        {
            TicketId = ticket.Id,
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            UserFullName = user.UserName ?? string.Empty,
            OrderId = ticket.OrderId,
            Category = ticket.Category.ToString(),
            Status = ticket.Status.ToString(),
            Subject = ticket.Subject,
            Description = ticket.Description
        }, cancellationToken);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        
        return !result
               ? Result<string>.Failure("An error occured saving the data")
               : Result<string>.Success(ticket.Id);
    }
}