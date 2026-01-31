using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Constants;
using SmartExpense.Core.Entities;
using SmartExpense.Core.Models;

namespace SmartExpense.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedDataAsync(AppDbContext context, UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, IOptions<AdminUserOptions> adminOptions, ILogger logger,
        IDateTimeProvider dateTimeProvider)
    {
        //Apply pending migrations
        await context.Database.MigrateAsync();

        //Seed roles if they don`t exist
        await SeedRolesAsync(roleManager);

        //Seed admin user if doesn't exist
        await SeedAdminUserAsync(userManager, adminOptions.Value, logger, dateTimeProvider);
        
        // Seed Categories if they don't exist
        await SeedCategoriesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        //Check if Admin role exists
        if (!await roleManager.RoleExistsAsync(IdentityRoleConstants.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>
            {
                Id = IdentityRoleConstants.AdminRoleGuid,
                Name = IdentityRoleConstants.Admin,
                NormalizedName = IdentityRoleConstants.Admin.ToUpper()
            });
        }

        if (!await roleManager.RoleExistsAsync(IdentityRoleConstants.User))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>
            {
                Id = IdentityRoleConstants.UserRoleGuid,
                Name = IdentityRoleConstants.User,
                NormalizedName = IdentityRoleConstants.User.ToUpper()
            });
        }
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var systemCategories = new List<Category>
        {
            new() { Name = "Food & Dining", Icon = "🍔", Color = "#FF6B6B", IsSystemCategory = true, UserId = null },
            new() { Name = "Transportation", Icon = "🚗", Color = "#4ECDC4", IsSystemCategory = true, UserId = null },
            new() { Name = "Housing", Icon = "🏠", Color = "#45B7D1", IsSystemCategory = true, UserId = null },
            new() { Name = "Utilities", Icon = "💡", Color = "#FFA07A", IsSystemCategory = true, UserId = null },
            new() { Name = "Entertainment", Icon = "🎬", Color = "#98D8C8", IsSystemCategory = true, UserId = null },
            new() { Name = "Shopping", Icon = "🛒", Color = "#F7DC6F", IsSystemCategory = true, UserId = null },
            new() { Name = "Healthcare", Icon = "💊", Color = "#BB8FCE", IsSystemCategory = true, UserId = null },
            new() { Name = "Education", Icon = "📚", Color = "#85C1E2", IsSystemCategory = true, UserId = null },
            new() { Name = "Salary", Icon = "💰", Color = "#52C41A", IsSystemCategory = true, UserId = null },
            new() { Name = "Investment", Icon = "📈", Color = "#1890FF", IsSystemCategory = true, UserId = null },
            new() { Name = "Gifts", Icon = "🎁", Color = "#EB2F96", IsSystemCategory = true, UserId = null },
            new() { Name = "Other", Icon = "➕", Color = "#8C8C8C", IsSystemCategory = true, UserId = null }
        };

        await context.Categories.AddRangeAsync(systemCategories);
        await context.SaveChangesAsync();
    }
    private static async Task SeedAdminUserAsync(UserManager<User> userManager, AdminUserOptions admin, ILogger logger,
        IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(admin.Email))
        {
            logger.LogWarning("AdminUser.Email is missing");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(admin.Email);
        if (existingAdmin != null)
        {
            logger.LogInformation($"Admin user {admin.Email} already exists.");
            if (!await userManager.IsInRoleAsync(existingAdmin, IdentityRoleConstants.Admin))
            {
                await userManager.AddToRoleAsync(existingAdmin, IdentityRoleConstants.Admin);
                logger.LogInformation("Added Admin role to existing admin user.");
            }

            return;
        }

        var user = User.Create(email: admin.Email, firstName: admin.FirstName, lastName: admin.LastName,
            createdAt: dateTimeProvider.UtcNow);

        var createResult = await userManager.CreateAsync(user, admin.Password);

        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to create Admin user.");
            foreach (var error in createResult.Errors)
            {
                logger.LogError($" - {error.Description}");
            }

            return;
        }

        await userManager.AddToRoleAsync(user, IdentityRoleConstants.Admin);
        logger.LogInformation("Admin user created successfully.");
        logger.LogInformation("Email: {Email}", admin.Email);
        logger.LogInformation("FirstName: {FirstName}", admin.FirstName);
        logger.LogInformation("LastName: {LastName}", admin.LastName);
    }
}