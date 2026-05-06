using System.Text.Json;
using API.Middleware;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UnitTests.API.Middleware;

public class ExceptionMiddlewareTests
{
    private static ILogger<ExceptionMiddleware> CreateLogger() =>
        new Mock<ILogger<ExceptionMiddleware>>().Object;

    private static Mock<IHostEnvironment> CreateEnv(string environmentName)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(environmentName);
        return env;
    }

    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_CallsNext()
    {
        var called = false;
        RequestDelegate next = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionMiddleware(next, CreateLogger(), CreateEnv("Development").Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionThrown_Returns400()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };
        RequestDelegate next = _ => throw new ValidationException(failures);

        var middleware = new ExceptionMiddleware(next, CreateLogger(), CreateEnv("Production").Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_Returns500()
    {
        RequestDelegate next = _ => throw new Exception("Something went wrong");

        var middleware = new ExceptionMiddleware(next, CreateLogger(), CreateEnv("Production").Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionInDevelopment_IncludesStackTrace()
    {
        RequestDelegate next = _ => throw new Exception("Dev error");

        var middleware = new ExceptionMiddleware(next, CreateLogger(), CreateEnv("Development").Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var doc = JsonDocument.Parse(body);
        var hasDetails = doc.RootElement.TryGetProperty("details", out var details);

        hasDetails.Should().BeTrue();
        // In development the stack trace is included (not null)
        details.ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionInProduction_ExcludesStackTrace()
    {
        RequestDelegate next = _ => throw new Exception("Prod error");

        var middleware = new ExceptionMiddleware(next, CreateLogger(), CreateEnv("Production").Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("details", out var details);

        details.ValueKind.Should().Be(JsonValueKind.Null);
    }
}
