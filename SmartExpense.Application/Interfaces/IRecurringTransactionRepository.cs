using SmartExpense.Core.Entities;

namespace SmartExpense.Application.Interfaces;

public interface IRecurringTransactionRepository : IGenericRepository<RecurringTransaction>
{
    Task<RecurringTransaction?> GetByIdForUserAsync(int id, Guid userId);
    Task<List<RecurringTransaction>> GetAllForUserAsync(Guid userId, bool? isActive = null);
    Task<List<RecurringTransaction>> GetDueForGenerationAsync(DateTime asOfDate);
}