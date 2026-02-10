using Microsoft.EntityFrameworkCore;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Core.Enums;
using SmartExpense.Core.Models;
using SmartExpense.Infrastructure.Data;

namespace SmartExpense.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Transaction>> GetPagedAsync(
        Guid userId,
        TransactionQueryParameters parameters)
    {
        IQueryable<Transaction> query = _dbSet
            .Where(t => t.UserId == userId)
            .Include(t => t.Category);

        // Apply filters
        query = ApplyFilters(query, parameters);

        // Apply sorting
        query = ApplySorting(query, parameters);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var data = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Transaction>
        {
            Data = data,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<Transaction>> GetRecentAsync(Guid userId, int count = 10)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .Include(t => t.Category)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAtUtc)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalIncomeAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbSet.Where(t =>
            t.UserId == userId &&
            t.TransactionType == TransactionType.Income);

        query = ApplyDateFilter(query, startDate, endDate);

        return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
    }

    public async Task<decimal> GetTotalExpenseAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbSet.Where(t =>
            t.UserId == userId &&
            t.TransactionType == TransactionType.Expense);

        query = ApplyDateFilter(query, startDate, endDate);

        return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
    }

    public async Task<int> GetTransactionCountAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbSet.Where(t => t.UserId == userId);
        query = ApplyDateFilter(query, startDate, endDate);
        return await query.CountAsync();
    }

    public async Task<Transaction?> GetByIdForUserAsync(int id, Guid userId)
    {
        return await _dbSet
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    #region Private Helper Methods

    private static IQueryable<Transaction> ApplyFilters(
        IQueryable<Transaction> query,
        TransactionQueryParameters parameters)
    {
        // Search term filter
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Description.ToLower().Contains(searchTerm) ||
                (t.Notes != null && t.Notes.ToLower().Contains(searchTerm)));
        }

        // Category filter
        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == parameters.CategoryId.Value);
        }

        // Transaction type filter
        if (parameters.TransactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == parameters.TransactionType.Value);
        }

        // Date range filter
        query = ApplyDateFilter(query, parameters.StartDate, parameters.EndDate);

        // Amount range filter
        if (parameters.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= parameters.MinAmount.Value);
        }

        if (parameters.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= parameters.MaxAmount.Value);
        }

        return query;
    }

    private static IQueryable<Transaction> ApplyDateFilter(
        IQueryable<Transaction> query,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        return query;
    }

    private static IQueryable<Transaction> ApplySorting(
        IQueryable<Transaction> query,
        TransactionQueryParameters parameters)
    {
        return parameters.SortBy.ToLower() switch
        {
            "amount" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Amount)
                : query.OrderBy(t => t.Amount),
            "description" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Description)
                : query.OrderBy(t => t.Description),
            "category" => parameters.SortDescending
                ? query.OrderByDescending(t => t.Category.Name)
                : query.OrderBy(t => t.Category.Name),
            _ => parameters.SortDescending
                ? query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAtUtc)
                : query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAtUtc)
        };
    }

    #endregion
}