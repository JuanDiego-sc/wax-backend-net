using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class DbInitializer(
    WriteDbContext context,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<DbInitializer> logger)
{
    private readonly WriteDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DbInitializer> _logger = logger;


    public async Task InitializeAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (await _roleManager.RoleExistsAsync(role)) continue;
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
                
            if (!result.Succeeded) _logger.LogError("Role {Role} not found", role);
            _logger.LogInformation("Role {Role} created", role);
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = _configuration["Admin:Email"];
        var adminPassword = _configuration["Admin:Password"];

        if (!string.IsNullOrEmpty(adminEmail) || !string.IsNullOrEmpty(adminPassword))
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };
            
            var result = await _userManager.CreateAsync(admin, adminPassword!);
            if (!result.Succeeded) _logger.LogError("Failed while creating user {User}", adminEmail);

            await _userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}