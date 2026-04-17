using Application.Interfaces.Services;
using Application.User.Queries;
using Domain.Entities;
using DomainUser = global::Domain.Entities.User;

namespace UnitTests.Application.User;

public class GetBillingAddressQueryHandlerTests
{
    private readonly Mock<IUserAccessor> _userAccessor = new();
    private readonly GetBillingAddressQueryHandler _handler;

    public GetBillingAddressQueryHandlerTests()
    {
        _handler = new GetBillingAddressQueryHandler(_userAccessor.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync((DomainUser?)null);

        var result = await _handler.Handle(new GetBillingAddressQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not  found");
    }

    [Fact]
    public async Task Handle_WhenBillingAddressMissing_ReturnsFailure()
    {
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync(new DomainUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "no-address",
                Email = "no-address@test.com",
                BillingAddress = null
            });

        var result = await _handler.Handle(new GetBillingAddressQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No billing address found for this user");
    }

    [Fact]
    public async Task Handle_WhenBillingAddressExists_ReturnsMappedDto()
    {
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync(new DomainUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "with-address",
                Email = "with-address@test.com",
                BillingAddress = new BillingAddress
                {
                    Name = "Jane Doe",
                    Line1 = "Street 1",
                    Line2 = "Apt 3",
                    City = "Quito",
                    State = "Pichincha",
                    PostalCode = "170101",
                    Country = "EC"
                }
            });

        var result = await _handler.Handle(new GetBillingAddressQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Jane Doe");
        result.Value.PostalCode.Should().Be("170101");
    }
}
