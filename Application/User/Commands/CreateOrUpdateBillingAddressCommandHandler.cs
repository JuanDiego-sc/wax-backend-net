using Application.Core;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Application.Orders.DTOs;
using Application.User.Extensions;
using Domain.Entities;
using Domain.Enumerators;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserDomain = Domain.Entities.User;

namespace Application.User.Commands;

public class CreateOrUpdateBillingAddressCommandHandler (
    IUserAccessor userAccessor,
    UserManager<UserDomain>  userManager,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateOrUpdateBillingAddressCommand, Result<BillingAddressDto>>
{
    public async Task<Result<BillingAddressDto>> Handle(CreateOrUpdateBillingAddressCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await userAccessor.GetUserWithBillingAddressAsync();
        if (user == null) return Result<BillingAddressDto>.Failure("User not found");

        if (user.BillingAddress == null)
        {
            user.BillingAddress = new BillingAddress
            {
                Name = request.BillingInfo.Name,
                Line1 = request.BillingInfo.Line1,
                Line2 = request.BillingInfo.Line2,
                City = request.BillingInfo.City,
                State = request.BillingInfo.State,
                PostalCode = request.BillingInfo.PostalCode,
                Country = request.BillingInfo.Country
            };
        }
        else
        {
            user.BillingAddress.Name = request.BillingInfo.Name;
            user.BillingAddress.Line1 = request.BillingInfo.Line1;
            user.BillingAddress.Line2 = request.BillingInfo.Line2;
            user.BillingAddress.City = request.BillingInfo.City;
            user.BillingAddress.State = request.BillingInfo.State;
            user.BillingAddress.PostalCode = request.BillingInfo.PostalCode;
            user.BillingAddress.Country = request.BillingInfo.Country;
        }

        user.IdentificationNumber = request.BillingInfo.IdentificationNumber;
        user.FirstName = request.BillingInfo.FirstName;
        user.LastName = request.BillingInfo.LastName;
        user.IdentificationType = request.BillingInfo.IdentificationType;
        user.Phone = request.BillingInfo.Phone;
        
        var roles = await userManager.GetRolesAsync(user);

        if (roles.Contains(Roles.Enrolled) && !roles.Contains(Roles.Registered))
        {
            await userManager.RemoveFromRoleAsync(user, Roles.Enrolled);
            await userManager.AddToRoleAsync(user, Roles.Registered);
        }

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        
        return !result
            ? Result<BillingAddressDto>.Failure("Failed to save billing address")
            : Result<BillingAddressDto>.Success(user.BillingAddress.ToDto());
    }
}