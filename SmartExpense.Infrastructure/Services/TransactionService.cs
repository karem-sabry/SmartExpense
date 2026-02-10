using SmartExpense.Application.Dtos.Transaction;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Core.Exceptions;
using SmartExpense.Core.Models;

namespace SmartExpense.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TransactionService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PagedResult<TransactionReadDto>> GetPagedAsync(
        Guid userId,
        TransactionQueryParameters parameters)
    {
        var pagedResult = await _unitOfWork.Transactions.GetPagedAsync(userId, parameters);

        return new PagedResult<TransactionReadDto>
        {
            Data = pagedResult.Data.Select(MapToReadDto).ToList(),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<TransactionReadDto> GetByIdAsync(int id, Guid userId)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdForUserAsync(id, userId);

        if (transaction == null)
            throw new NotFoundException("Transaction", id);

        return MapToReadDto(transaction);
    }

    public async Task<List<TransactionReadDto>> GetRecentAsync(Guid userId, int count = 10)
    {
        var transactions = await _unitOfWork.Transactions.GetRecentAsync(userId, count);
        return transactions.Select(MapToReadDto).ToList();
    }

    public async Task<TransactionSummaryDto> GetSummaryAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate)
    {
        var totalIncome = await _unitOfWork.Transactions.GetTotalIncomeAsync(userId, startDate, endDate);
        var totalExpense = await _unitOfWork.Transactions.GetTotalExpenseAsync(userId, startDate, endDate);
        var transactionCount = await _unitOfWork.Transactions.GetTransactionCountAsync(userId, startDate, endDate);

        return new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            TransactionCount = transactionCount,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<TransactionReadDto> CreateAsync(TransactionCreateDto dto, Guid userId)
    {
        // Validate category exists and user has access to it
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(dto.CategoryId, userId);
        if (category == null)
            throw new NotFoundException("Category", dto.CategoryId);

        if (!category.IsActive)
            throw new ValidationException("Cannot create transaction with inactive category");

        // Validate transaction date is not in future
        if (dto.TransactionDate > _dateTimeProvider.UtcNow)
            throw new ValidationException("Transaction date cannot be in the future");

        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Amount = dto.Amount,
            TransactionType = dto.TransactionType,
            TransactionDate = dto.TransactionDate,
            Notes = dto.Notes
            // CreatedAtUtc and CreatedBy will be set by AuditInterceptor
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Reload to get category navigation property
        var created = await _unitOfWork.Transactions.GetByIdForUserAsync(transaction.Id, userId);
        return MapToReadDto(created!);
    }

    public async Task<TransactionReadDto> UpdateAsync(int id, TransactionUpdateDto dto, Guid userId)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdForUserAsync(id, userId);

        if (transaction == null)
            throw new NotFoundException("Transaction", id);

        // Validate category exists and user has access to it
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(dto.CategoryId, userId);
        if (category == null)
            throw new NotFoundException("Category", dto.CategoryId);

        if (!category.IsActive)
            throw new ValidationException("Cannot update transaction with inactive category");

        // Validate transaction date is not in future
        if (dto.TransactionDate > _dateTimeProvider.UtcNow)
            throw new ValidationException("Transaction date cannot be in the future");

        // Update transaction
        transaction.CategoryId = dto.CategoryId;
        transaction.Description = dto.Description;
        transaction.Amount = dto.Amount;
        transaction.TransactionType = dto.TransactionType;
        transaction.TransactionDate = dto.TransactionDate;
        transaction.Notes = dto.Notes;
        // UpdatedAtUtc and UpdatedBy will be set by AuditInterceptor

        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Reload to get updated category navigation property
        var updated = await _unitOfWork.Transactions.GetByIdForUserAsync(id, userId);
        return MapToReadDto(updated!);
    }

    public async Task DeleteAsync(int id, Guid userId)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdForUserAsync(id, userId);

        if (transaction == null)
            throw new NotFoundException("Transaction", id);

        await _unitOfWork.Transactions.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    #region Private Helper Methods

    private static TransactionReadDto MapToReadDto(Transaction transaction)
    {
        return new TransactionReadDto
        {
            Id = transaction.Id,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name ?? string.Empty,
            CategoryIcon = transaction.Category?.Icon,
            CategoryColor = transaction.Category?.Color,
            Description = transaction.Description,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType,
            TransactionTypeDisplay = transaction.TransactionType.ToString(),
            TransactionDate = transaction.TransactionDate,
            Notes = transaction.Notes,
            CreatedAtUtc = transaction.CreatedAtUtc,
            UpdatedAtUtc = transaction.UpdatedAtUtc
        };
    }

    #endregion
}