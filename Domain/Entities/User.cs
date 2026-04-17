using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser
{
    public string? IdentificationNumber { get; set; }
    public string? IdentificationType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }

    #region Navigation Properties
    public string? BillingAddressId { get; init; }
    public BillingAddress? BillingAddress { get; set; }
    #endregion
}
