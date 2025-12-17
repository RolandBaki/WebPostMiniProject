using DyntellProject.Core.Entities;
using DyntellProject.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DyntellProject.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DataSeeder");

        const string adminUsername = "admin";
        const string adminEmail = "admin@dyntellproject.com";
        const string adminPassword = "Admin123";

        // ellenorzees ha letezik mar
        var existingAdmin = await userManager.FindByNameAsync(adminUsername);
        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists.");
            return;
        }

        // admin letrehozas
        var adminUser = new User
        {
            UserName = adminUsername,
            Email = adminEmail,
            AgeGroup = AgeGroup.Felnott,
            Role = UserRole.Admin 
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            logger.LogInformation("Admin user created successfully. Username: {Username}, Password: {Password}", 
                adminUsername, adminPassword);
        }
        else
        {
            logger.LogError("Failed to create admin user: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

