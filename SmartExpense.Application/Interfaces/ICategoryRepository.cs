using SmartExpense.Core.Entities;

namespace SmartExpense.Application.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetAllForUserAsync(Guid userId);
    Task<Category?> GetByIdForUserAsync(int id, Guid userId);
    Task<bool> CategoryNameExistsAsync(Guid userId, string name, int? excludeId = null);
}