using DomainUser = Domain.Entities.User;

namespace Application.Interfaces.Services;

public interface IUserAccessor
{
    string? GetUserId();
    Task<DomainUser?> GetUserAsync();
    Task<DomainUser?> GetUserWithBillingAddressAsync();
    Task<IList<string>> GetUserRolesAsync();
}
