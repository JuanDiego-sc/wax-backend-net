using System;
using Domain.Entities;
using DomainUser = Domain.Entities.User;

namespace Application.Interfaces;

public interface IUserAccessor
{
    string GetUserId();
    Task<DomainUser> GetUserAsync();
}
