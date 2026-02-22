using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser
{
    public int? AddressId { get; set; }
    public Address? Address { get; set; } 
}
