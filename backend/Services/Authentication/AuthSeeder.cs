using Lexicon.Services.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Lexicon.Services.Auth;

/// <summary>
/// Ensures the Guest, Editor, and Admin roles exist in the database.
/// Safe to call on every startup (idempotent).
/// </summary>
public class AuthSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AuthSeeder> _logger;

    public AuthSeeder(RoleManager<IdentityRole> roleManager, ILogger<AuthSeeder> logger)
    {
        _roleManager = roleManager;
        _logger      = logger;
    }

    public async Task SeedRolesAsync()
    {
        await CreateRoleIfNotExistsAsync(Roles.Guest);
        await CreateRoleIfNotExistsAsync(Roles.Editor);
        await CreateRoleIfNotExistsAsync(Roles.Admin);
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
                _logger.LogInformation("Role '{RoleName}' created.", roleName);
            else
                _logger.LogError(
                    "Failed to create role '{RoleName}': {Errors}",
                    roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}