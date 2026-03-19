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
    public async Task<ActionResult<string>> CreateSupportTicket(CreateSupportTicketDto supportTicket)
    {
        return await HandleCommand(new CreateSupportTicketCommand { TicketDto = supportTicket });
    }
    
    #endregion

    #region HttpPut methods

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupportTicket(string id, UpdateSupportTicketDto supportTicket)
    {
        return await HandleCommand(new UpdateSupportTicketCommand { TicketId = id, TicketDto = supportTicket });
    }

    #endregion

    #region HttpDelete methods

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupportTicket(string id)
    {
        return await HandleCommand(new DeleteSupportTicketCommand { TicketId = id });
    }

    #endregion
}