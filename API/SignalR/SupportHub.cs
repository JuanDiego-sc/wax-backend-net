using System.Security.Claims;
using Application.Comments.Commands;
using Application.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class SupportHub(IMediator mediator) : Hub
{
    public async Task SendComment(AddCommentCommand command)
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized");
        
        var comment = await mediator.Send(command);
        
        await Clients.Group(command.TicketId).SendAsync("CommentAdded", comment.Value);
        
    }
    
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var ticketId = httpContext?.Request.Query["ticketId"].ToString();

        if (string.IsNullOrEmpty(ticketId))
            throw new HubException("TicketId is required");

        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);

        var result = await mediator.Send(new GetCommentsQuery() { TicketId = ticketId });

        await Clients.Caller.SendAsync("LoadComments", result.Value);
    }
}