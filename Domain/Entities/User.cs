using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser
{
    public string? AddressId { get; set; }
    public Address? Address { get; set; } 
}
