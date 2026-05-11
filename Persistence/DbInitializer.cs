using Domain.Entities;
using Domain.Enumerators;
using Domain.ProductAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class DbInitializer(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    WriteDbContext context,
    ILogger<DbInitializer> logger)
{
    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedQuotationRulesAsync();
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

    private async Task SeedQuotationRulesAsync()
    {
        if (!await context.QuotationRules.AnyAsync())
        {
            context.QuotationRules.AddRange(
                new QuotationRule { Key = "BASE_COST", Value = 5000m, Description = "Costo base en centavos" },
                new QuotationRule { Key = "MARGIN_MULTIPLIER", Value = 1.6m, Description = "Multiplicador de margen" },
                new QuotationRule { Key = "DEFAULT_DEPTH_CM", Value = 5m, Description = "Profundidad por defecto" },
                new QuotationRule { Key = "MATERIAL_default", Value = 2m, Description = "Costo en centavos por cm3" }
                );
            await context.SaveChangesAsync();
        }
    }
}