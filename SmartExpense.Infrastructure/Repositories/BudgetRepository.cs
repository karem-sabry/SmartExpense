using Microsoft.EntityFrameworkCore;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Infrastructure.Data;

namespace SmartExpense.Infrastructure.Repositories;

public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
{
    public BudgetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Budget?> GetByIdForUserAsync(int id, Guid userId)
    {
        return await _dbSet
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<List<Budget>> GetAllForUserAsync(Guid userId, int? month, int? year)
    {
        IQueryable<Budget> query = _dbSet
            .Where(b => b.UserId == userId)
            .Include(b => b.Category);

        if (month.HasValue)
        {
            query = query.Where(b => b.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(b => b.Year == year.Value);
        }

        return await query
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ThenBy(b => b.Category.Name)
            .ToListAsync();
    }

    public async Task<Budget?> GetByCategoryAndPeriodAsync(Guid userId, int categoryId, int month, int year)
    {
        return await _dbSet
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.CategoryId == categoryId &&
                b.Month == month &&
                b.Year == year);
    }

    public async Task<bool> BudgetExistsAsync(
        Guid userId,
        int categoryId,
        int month,
        int year,
        int? excludeBudgetId = null)
    {
        var query = _dbSet.Where(b =>
            b.UserId == userId &&
            b.CategoryId == categoryId &&
            b.Month == month &&
            b.Year == year);

        if (excludeBudgetId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBudgetId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<Budget>> GetByMonthYearAsync(Guid userId, int month, int year)
    {
        return await _dbSet
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .Include(b => b.Category)
            .ToListAsync();
    }
}