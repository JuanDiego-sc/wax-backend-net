using System.Reflection.Metadata;
using Application.Core.Pagination;
using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using Application.SupportAssist.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class SupportController : BaseApiController
{
    #region HttpGet Methods

    [HttpGet]
    public async Task<ActionResult<PagedList<SupportTicketDto>>> GetSupportTickets(SupportTicketParams supportTicketParams)
    {
        return await HandleQuery(new GetSupportTicketsQuery { TicketParams = supportTicketParams });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupportTicketDto>> GetSupportTicket(string id)
    {
        return await HandleQuery(new GetSupportTicketDetailsQuery { TicketId = id });
    }

    #endregion
   
    #region HttpPost methods

    [HttpPost]
    public async Task<ActionResult<CreateSupportTicketDto>> CreateSupportTicket(CreateSupportTicketDto supportTicket)
    {
        return await HandleCommand(new CreateSupportTicketCommand { TicketDto = supportTicket });
    }
    #endregion
}