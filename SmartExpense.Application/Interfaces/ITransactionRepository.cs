using SmartExpense.Core.Entities;
using SmartExpense.Core.Models;

namespace SmartExpense.Application.Interfaces;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<PagedResult<Transaction>> GetPagedAsync(Guid userId, TransactionQueryParameters parameters);
    Task<List<Transaction>> GetRecentAsync(Guid userId, int count = 10);
    Task<decimal> GetTotalIncomeAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<decimal> GetTotalExpenseAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<int> GetTransactionCountAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<Transaction?> GetByIdForUserAsync(int id, Guid userId);
}