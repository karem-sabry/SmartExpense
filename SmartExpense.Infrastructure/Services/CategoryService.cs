using SmartExpense.Application.Dtos.Category;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Core.Exceptions;

namespace SmartExpense.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CategoryService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
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
            throw new NotFoundException("Category", id);

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
        var exists = await _unitOfWork.Categories.CategoryNameExistsAsync(userId, dto.Name);

        if (exists)
            throw new ConflictException("Category with this name already exists");


        var category = new Category
        {
            UserId = userId,
            Name = dto.Name,
            Icon = dto.Icon,
            Color = dto.Color,
            IsSystemCategory = false,
            IsActive = true,
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
            throw new NotFoundException("Category", id);

        if (category.IsSystemCategory)
            throw new ForbiddenException("Cannot update system categories");

        var nameExists = await _unitOfWork.Categories.CategoryNameExistsAsync(userId, dto.Name, id);

        if (nameExists)
            throw new ConflictException("Category with this name already exists");

        category.Name = dto.Name;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.IsActive = dto.IsActive;

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
            throw new NotFoundException("Category", id);

        if (category.IsSystemCategory)
            throw new ForbiddenException("Cannot delete system categories");

        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}