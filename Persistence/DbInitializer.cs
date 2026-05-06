using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class DbInitializer(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<DbInitializer> logger)
{
    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;
            var result = await roleManager.CreateAsync(new IdentityRole(role));
                
            if (!result.Succeeded) logger.LogError("Role {Role} not found", role);
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (string.IsNullOrEmpty(adminEmail) && string.IsNullOrEmpty(adminPassword))
        {
            logger.LogWarning("Admin:Email o Admin:Password no configurados. Seed de admin omitido.");
            return;
        }
        
        var admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
        };
            
        var result = await userManager.CreateAsync(admin, adminPassword!);
        if (!result.Succeeded) logger.LogError("Failed while creating user {User}", adminEmail);

        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}