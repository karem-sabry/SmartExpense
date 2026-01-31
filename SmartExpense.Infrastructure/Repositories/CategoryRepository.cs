using Microsoft.EntityFrameworkCore;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Infrastructure.Data;

namespace SmartExpense.Infrastructure.Repositories;

public class CategoryRepository: GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetAllForUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(c => c.IsSystemCategory || c.UserId == userId)
            .OrderBy(c => c.IsSystemCategory ? 0 : 1) // System categories first
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdForUserAsync(int id, Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Id == id && (c.IsSystemCategory || c.UserId == userId));
    }

    public async Task<bool> CategoryNameExistsAsync(Guid userId, string name, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}