using SmartExpense.Application.Dtos.Budget;

namespace SmartExpense.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetReadDto>> GetAllAsync(Guid userId, int? month, int? year);
    Task<BudgetReadDto> GetByIdAsync(int id, Guid userId);
    Task<BudgetSummaryDto> GetSummaryAsync(Guid userId, int month, int year);
    Task<BudgetReadDto> CreateAsync(BudgetCreateDto dto, Guid userId);
    Task<BudgetReadDto> UpdateAsync(int id, BudgetUpdateDto dto, Guid userId);
    Task DeleteAsync(int id, Guid userId);
}