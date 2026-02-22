using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>()
            ?? throw new InvalidOperationException("Mediator service is not registered or is not working");

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if(!result.IsSuccess && result.Code == 404) return NotFound();
            if(result.IsSuccess && result.Value != null) return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
