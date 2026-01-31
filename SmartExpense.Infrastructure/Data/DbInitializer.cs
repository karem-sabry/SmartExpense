using Microsoft.EntityFrameworkCore;
using SmartExpense.Core.Entities;

namespace SmartExpense.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        if (await context.Categories.AnyAsync(c => c.IsSystemCategory))
            return;

        // Seed system categories
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
}