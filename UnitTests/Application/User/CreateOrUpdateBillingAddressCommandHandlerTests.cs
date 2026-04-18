using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Application.User.Commands;
using Application.User.DTOs;
using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Identity;
using DomainUser = global::Domain.Entities.User;

namespace UnitTests.Application.User;

public class CreateOrUpdateBillingAddressCommandHandlerTests
{
    private readonly Mock<IUserAccessor> _userAccessor = new();
    private readonly Mock<UserManager<DomainUser>> _userManager;
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateOrUpdateBillingAddressCommandHandler _handler;

    public CreateOrUpdateBillingAddressCommandHandlerTests()
    {
        _userManager = CreateUserManagerMock();
        _handler = new CreateOrUpdateBillingAddressCommandHandler(
            _userAccessor.Object,
            _userManager.Object,
            _unitOfWork.Object);
    }

    private static Mock<UserManager<DomainUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<DomainUser>>();

        return new Mock<UserManager<DomainUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static CreateOrUpdateBillingAddressCommand CreateCommand(
        string name = "Jane Doe",
        string line1 = "Street 123",
        string city = "Quito")
    {
        return new CreateOrUpdateBillingAddressCommand
        {
            BillingInfo = new CreateOrUpdateBillingInfoRequest
            {
                Name = name,
                Line1 = line1,
                Line2 = "Apt 4",
                City = city,
                State = "Pichincha",
                PostalCode = "170101",
                Country = "EC",
                IdentificationNumber = "1723456789",
                IdentificationType = "CI",
                FirstName = "Jane",
                LastName = "Doe",
                Phone = "+593999999999"
            }
        };
    }

    private static DomainUser CreateUser(bool withAddress)
    {
        return new DomainUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "billing-user",
            Email = "billing@test.com",
            BillingAddress = withAddress
                ? new BillingAddress
                {
                    Name = "Old Name",
                    Line1 = "Old Line 1",
                    Line2 = "Old Line 2",
                    City = "Old City",
                    State = "Old State",
                    PostalCode = "0000",
                    Country = "Old Country"
                }
                : null
        };
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync((DomainUser?)null);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoBillingAddress_CreatesAddress_AndPromotesRole()
    {
        var user = CreateUser(withAddress: false);
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync(user);
        _userManager
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync([Roles.Enrolled]);
        _userManager
            .Setup(m => m.RemoveFromRoleAsync(user, Roles.Enrolled))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(m => m.AddToRoleAsync(user, Roles.Registered))
            .ReturnsAsync(IdentityResult.Success);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = CreateCommand(name: "New Name", city: "Guayaquil");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.BillingAddress.Should().NotBeNull();
        user.BillingAddress!.Name.Should().Be("New Name");
        user.BillingAddress.City.Should().Be("Guayaquil");
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Doe");
        user.IdentificationNumber.Should().Be("1723456789");
        user.IdentificationType.Should().Be("CI");
        user.Phone.Should().Be("+593999999999");
        _userManager.Verify(m => m.RemoveFromRoleAsync(user, Roles.Enrolled), Times.Once);
        _userManager.Verify(m => m.AddToRoleAsync(user, Roles.Registered), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasBillingAddress_UpdatesAddress_AndDoesNotSwitchRoles()
    {
        var user = CreateUser(withAddress: true);
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync(user);
        _userManager
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync([Roles.Enrolled, Roles.Registered]);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = CreateCommand(name: "Updated Name", line1: "Updated Line", city: "Cuenca");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.BillingAddress!.Name.Should().Be("Updated Name");
        user.BillingAddress.Line1.Should().Be("Updated Line");
        user.BillingAddress.City.Should().Be("Cuenca");
        _userManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<DomainUser>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<DomainUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        var user = CreateUser(withAddress: true);
        _userAccessor
            .Setup(a => a.GetUserWithBillingAddressAsync())
            .ReturnsAsync(user);
        _userManager
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync([Roles.Registered]);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to save billing address");
    }
}
