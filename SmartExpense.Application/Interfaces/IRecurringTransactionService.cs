using SmartExpense.Application.Dtos.RecurringTransaction;

namespace SmartExpense.Application.Interfaces;

public interface IRecurringTransactionService
{
    Task<List<RecurringTransactionReadDto>> GetAllAsync(Guid userId, bool? isActive = null);
    Task<RecurringTransactionReadDto> GetByIdAsync(int id, Guid userId);
    Task<RecurringTransactionReadDto> CreateAsync(RecurringTransactionCreateDto dto, Guid userId);
    Task<RecurringTransactionReadDto> UpdateAsync(int id, RecurringTransactionUpdateDto dto, Guid userId);
    Task DeleteAsync(int id, Guid userId);
    Task<RecurringTransactionReadDto> ToggleActiveAsync(int id, Guid userId);
    Task<GenerateTransactionsResultDto> GenerateTransactionsAsync(Guid userId);
    Task<GenerateTransactionsResultDto> GenerateForRecurringTransactionAsync(int recurringId, Guid userId);
}