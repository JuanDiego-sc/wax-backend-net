using Application.Core;
using FluentValidation;
using MediatR;

namespace UnitTests.Application.Core;

public class ValidationBehaviorTests
{
    public record TestRequest(string Value) : IRequest<Result<string>>;

    private sealed class PassingValidator : AbstractValidator<TestRequest>
    {
        public PassingValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    private sealed class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator()
        {
            RuleFor(x => x.Value).Must(_ => false).WithMessage("Always fails");
        }
    }

    [Fact]
    public async Task Handle_WhenNoValidator_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(validator: null);
        var request = new TestRequest("data");
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = async (ct) =>
        {
            nextCalled = true;
            return await Task.FromResult(Result<string>.Success("ok"));
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorPasses_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(new PassingValidator());
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = async (ct) =>
        {
            nextCalled = true;
            return await Task.FromResult(Result<string>.Success("ok"));
        };

        await behavior.Handle(new TestRequest("value"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, Result<string>>(new FailingValidator());

        RequestHandlerDelegate<Result<string>> next = async (ct) =>
            await Task.FromResult(Result<string>.Success("ok"));

        var act = async () => await behavior.Handle(new TestRequest("anything"), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
