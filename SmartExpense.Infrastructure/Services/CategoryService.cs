using SmartExpense.Application.Dtos.Category;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;

namespace SmartExpense.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CategoryService(IUnitOfWork unitOfWork,IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<CategoryReadDto>> GetAllAsync(Guid userId)
    {
        var categories = await _unitOfWork.Categories.GetAllForUserAsync(userId);

        return categories.Select(c => new CategoryReadDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            IsSystemCategory = c.IsSystemCategory,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<CategoryReadDto> GetByIdAsync(int id, Guid userId)
    {
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(id, userId);

        if (category == null)
            throw new InvalidOperationException("Category not found");

        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color,
            IsSystemCategory = category.IsSystemCategory,
            IsActive = category.IsActive
        };
    }

    public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto, Guid userId)
    {
        // Check if category name already exists for this user
        var exists = await _unitOfWork.Categories.CategoryNameExistsAsync(userId, dto.Name);

        if (exists)
            throw new InvalidOperationException("Category with this name already exists");

        var category = new Category
        {
            UserId = userId,
            Name = dto.Name,
            Icon = dto.Icon,
            Color = dto.Color,
            IsSystemCategory = false,
            IsActive = true,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color,
            IsSystemCategory = category.IsSystemCategory,
            IsActive = category.IsActive
        };
    }

    public async Task<CategoryReadDto> UpdateAsync(int id, CategoryUpdateDto dto, Guid userId)
    {
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(id, userId);

        if (category == null)
            throw new InvalidOperationException("Category not found or you don't have permission to update it");

        if (category.IsSystemCategory)
            throw new InvalidOperationException("Cannot update system categories");

        // Check if new name already exists for this user (excluding current category)
        var nameExists = await _unitOfWork.Categories.CategoryNameExistsAsync(userId, dto.Name, id);

        if (nameExists)
            throw new InvalidOperationException("Category with this name already exists");

        category.Name = dto.Name;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.IsActive = dto.IsActive;
        category.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color,
            IsSystemCategory = category.IsSystemCategory,
            IsActive = category.IsActive
        };
    }

    public async Task DeleteAsync(int id, Guid userId)
    {
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(id, userId);

        if (category == null)
            throw new InvalidOperationException("Category not found or you don't have permission to delete it");

        if (category.IsSystemCategory)
            throw new InvalidOperationException("Cannot delete system categories");

        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}