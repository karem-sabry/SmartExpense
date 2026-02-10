using Microsoft.EntityFrameworkCore;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Infrastructure.Data;

namespace SmartExpense.Infrastructure.Repositories;

public class RecurringTransactionRepository : GenericRepository<RecurringTransaction>, IRecurringTransactionRepository
{
    public RecurringTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RecurringTransaction?> GetByIdForUserAsync(int id, Guid userId)
    {
        return await _dbSet
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<List<RecurringTransaction>> GetAllForUserAsync(Guid userId, bool? isActive = null)
    {
        IQueryable<RecurringTransaction> query = _dbSet
            .Where(r => r.UserId == userId)
            .Include(r => r.Category);

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(r => r.IsActive)
            .ThenBy(r => r.Description)
            .ToListAsync();
    }

    public async Task<List<RecurringTransaction>> GetDueForGenerationAsync(DateTime asOfDate)
    {
        return await _dbSet
            .Include(r => r.Category)
            .Where(r =>
                r.IsActive &&
                r.StartDate <= asOfDate &&
                (r.EndDate == null || r.EndDate >= asOfDate)
            )
            .ToListAsync();
    }
}