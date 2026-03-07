
using API.Logging;
using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        private IMediator Mediator => field ??= HttpContext.RequestServices.GetRequiredService<IMediator>()
            ?? throw new InvalidOperationException("Mediator service is not registered or is not working");

        private ILogger Logger => field ??= HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger(GetType());

        protected async Task<ActionResult> HandleCommand<T>(IRequest<Result<T>> command)
        {
            Logger.SendingCommand(command.GetType().Name, command);
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        protected async Task<ActionResult> HandleQuery<T>(IRequest<Result<T>> query)
        {
            Logger.SendingQuery(query.GetType().Name, query);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        private ActionResult HandleResult<T>(Result<T> result) =>
            result switch
            {
                { IsSuccess: false, Code: 404 } => NotFound(),
                { IsSuccess: true, Value: not null } => Ok(result.Value),
                _ => BadRequest(result.Error)
            };
    }
}
