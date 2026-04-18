using Application.Core;
using Application.Interfaces.Services;
using Application.Orders.DTOs;
using Application.User.Extensions;
using MediatR;

namespace Application.User.Queries;

public class GetBillingAddressQueryHandler(IUserAccessor userAccessor)
    : IRequestHandler<GetBillingAddressQuery, Result<BillingAddressDto>>
{
    public async Task<Result<BillingAddressDto>> Handle(GetBillingAddressQuery request, CancellationToken cancellationToken)
    {
        var user = await userAccessor.GetUserWithBillingAddressAsync();
        if(user == null) return Result<BillingAddressDto>.Failure("User not  found");
        
        return user.BillingAddress == null 
            ? Result<BillingAddressDto>.Failure("No billing address found for this user") 
            : Result<BillingAddressDto>.Success(user.BillingAddress.ToDto());
    }
}