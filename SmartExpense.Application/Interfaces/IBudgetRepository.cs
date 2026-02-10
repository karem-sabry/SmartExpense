using SmartExpense.Core.Entities;

namespace SmartExpense.Application.Interfaces;

public interface IBudgetRepository : IGenericRepository<Budget>
{
    Task<Budget?> GetByIdForUserAsync(int id, Guid userId);
    Task<List<Budget>> GetAllForUserAsync(Guid userId, int? month, int? year);
    Task<Budget?> GetByCategoryAndPeriodAsync(Guid userId, int categoryId, int month, int year);
    Task<bool> BudgetExistsAsync(Guid userId, int categoryId, int month, int year, int? excludeBudgetId = null);
    Task<List<Budget>> GetByMonthYearAsync(Guid userId, int month, int year);
}