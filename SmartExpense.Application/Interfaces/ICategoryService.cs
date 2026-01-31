using SmartExpense.Application.Dtos.Category;

namespace SmartExpense.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryReadDto>> GetAllAsync(Guid userId);
    Task<CategoryReadDto> GetByIdAsync(int id, Guid userId);
    Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto, Guid userId);
    Task<CategoryReadDto> UpdateAsync(int id, CategoryUpdateDto dto, Guid userId);
    Task DeleteAsync(int id, Guid userId);
}