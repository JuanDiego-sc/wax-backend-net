using API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTests.Helpers;

public static class ControllerTestFactory
{
    /// <summary>
    /// Wires up a controller's HttpContext so that BaseApiController can resolve
    /// IMediator and ILoggerFactory from the service provider — mimicking how ASP.NET Core
    /// injects them at runtime without using the full middleware pipeline.
    /// </summary>
    public static (Mock<IMediator> Mediator, TController Controller) Create<TController>(
        TController controller) where TController : BaseApiController
    {
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(logger.Object);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        services.AddSingleton(mediator.Object);
        services.AddSingleton(loggerFactory.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            }
        };

        return (mediator, controller);
    }

    /// <summary>
    /// Same as Create but also sets a ClaimsPrincipal on the request,
    /// used for tests that check User.Identity.IsAuthenticated.
    /// </summary>
    public static (Mock<IMediator> Mediator, TController Controller) CreateAuthenticated<TController>(
        TController controller,
        System.Security.Claims.ClaimsPrincipal user) where TController : BaseApiController
    {
        var (mediator, ctrl) = Create(controller);
        ctrl.ControllerContext.HttpContext.User = user;
        return (mediator, ctrl);
    }
}
