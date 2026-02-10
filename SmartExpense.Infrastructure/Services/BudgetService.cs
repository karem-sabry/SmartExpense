using System.Globalization;
using SmartExpense.Application.Dtos.Budget;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Entities;
using SmartExpense.Core.Enums;
using SmartExpense.Core.Exceptions;

namespace SmartExpense.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BudgetService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<BudgetReadDto>> GetAllAsync(Guid userId, int? month, int? year)
    {
        var budgets = await _unitOfWork.Budgets.GetAllForUserAsync(userId, month, year);
        var budgetDtos = new List<BudgetReadDto>();

        foreach (var budget in budgets)
        {
            var dto = await MapToReadDtoAsync(budget, userId);
            budgetDtos.Add(dto);
        }

        return budgetDtos;
    }

    public async Task<BudgetReadDto> GetByIdAsync(int id, Guid userId)
    {
        var budget = await _unitOfWork.Budgets.GetByIdForUserAsync(id, userId);

        if (budget == null)
            throw new NotFoundException("Budget", id);

        return await MapToReadDtoAsync(budget, userId);
    }

    public async Task<BudgetSummaryDto> GetSummaryAsync(Guid userId, int month, int year)
    {
        var budgets = await _unitOfWork.Budgets.GetByMonthYearAsync(userId, month, year);
        var budgetDtos = new List<BudgetReadDto>();

        decimal totalBudgeted = 0;
        decimal totalSpent = 0;
        int budgetsExceeded = 0;
        int budgetsApproaching = 0;

        foreach (var budget in budgets)
        {
            var dto = await MapToReadDtoAsync(budget, userId);
            budgetDtos.Add(dto);

            totalBudgeted += dto.Amount;
            totalSpent += dto.Spent;

            if (dto.Status == BudgetStatus.Exceeded)
                budgetsExceeded++;
            else if (dto.Status == BudgetStatus.Approaching)
                budgetsApproaching++;
        }

        var percentageUsed = totalBudgeted > 0 ? (totalSpent / totalBudgeted) * 100 : 0;

        return new BudgetSummaryDto
        {
            Month = month,
            Year = year,
            Period = GetPeriodDisplay(month, year),
            TotalBudgeted = totalBudgeted,
            TotalSpent = totalSpent,
            TotalRemaining = totalBudgeted - totalSpent,
            PercentageUsed = Math.Round(percentageUsed, 2),
            TotalBudgets = budgets.Count,
            BudgetsExceeded = budgetsExceeded,
            BudgetsApproaching = budgetsApproaching,
            Budgets = budgetDtos
        };
    }

    public async Task<BudgetReadDto> CreateAsync(BudgetCreateDto dto, Guid userId)
    {
        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdForUserAsync(dto.CategoryId, userId);
        if (category == null)
            throw new NotFoundException("Category", dto.CategoryId);

        // Check if budget already exists for this category and period
        var exists = await _unitOfWork.Budgets.BudgetExistsAsync(
            userId,
            dto.CategoryId,
            dto.Month,
            dto.Year
        );

        if (exists)
            throw new ConflictException(
                $"Budget already exists for {category.Name} in {GetPeriodDisplay(dto.Month, dto.Year)}");

        // Validate period is not in the past (optional - you can allow past budgets)
        var budgetDate = new DateTime(dto.Year, dto.Month, 1);
        var currentDate = new DateTime(_dateTimeProvider.UtcNow.Year, _dateTimeProvider.UtcNow.Month, 1);

        if (budgetDate < currentDate)
            throw new ValidationException("Cannot create budget for past months");

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Month = dto.Month,
            Year = dto.Year
        };

        await _unitOfWork.Budgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Budgets.GetByIdForUserAsync(budget.Id, userId);
        return await MapToReadDtoAsync(created!, userId);
    }

    public async Task<BudgetReadDto> UpdateAsync(int id, BudgetUpdateDto dto, Guid userId)
    {
        var budget = await _unitOfWork.Budgets.GetByIdForUserAsync(id, userId);

        if (budget == null)
            throw new NotFoundException("Budget", id);

        budget.Amount = dto.Amount;

        await _unitOfWork.Budgets.UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Budgets.GetByIdForUserAsync(id, userId);
        return await MapToReadDtoAsync(updated!, userId);
    }

    public async Task DeleteAsync(int id, Guid userId)
    {
        var budget = await _unitOfWork.Budgets.GetByIdForUserAsync(id, userId);

        if (budget == null)
            throw new NotFoundException("Budget", id);

        await _unitOfWork.Budgets.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    #region Private Helper Methods

    private async Task<BudgetReadDto> MapToReadDtoAsync(Budget budget, Guid userId)
    {
        // Calculate spent amount for this category in this month/year
        var startDate = new DateTime(budget.Year, budget.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var spent = await _unitOfWork.Transactions.GetTotalExpenseAsync(
            userId,
            startDate,
            endDate
        );

        // Filter by category
        var categoryTransactions = await _unitOfWork.Transactions.GetPagedAsync(
            userId,
            new Core.Models.TransactionQueryParameters
            {
                CategoryId = budget.CategoryId,
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = TransactionType.Expense,
                PageSize = int.MaxValue
            }
        );

        var actualSpent = categoryTransactions.Data.Sum(t => t.Amount);
        var remaining = budget.Amount - actualSpent;
        var percentageUsed = budget.Amount > 0 ? (actualSpent / budget.Amount) * 100 : 0;

        // Determine status
        var status = GetBudgetStatus(percentageUsed);

        return new BudgetReadDto
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name ?? string.Empty,
            CategoryIcon = budget.Category?.Icon,
            CategoryColor = budget.Category?.Color,
            Amount = budget.Amount,
            Month = budget.Month,
            Year = budget.Year,
            Period = GetPeriodDisplay(budget.Month, budget.Year),
            Spent = actualSpent,
            Remaining = remaining,
            PercentageUsed = Math.Round(percentageUsed, 2),
            Status = status,
            StatusDisplay = status.ToString(),
            CreatedAtUtc = budget.CreatedAtUtc,
            UpdatedAtUtc = budget.UpdatedAtUtc
        };
    }

    private static BudgetStatus GetBudgetStatus(decimal percentageUsed)
    {
        if (percentageUsed >= 100)
            return BudgetStatus.Exceeded;
        if (percentageUsed >= 80)
            return BudgetStatus.Approaching;
        return BudgetStatus.UnderBudget;
    }

    private static string GetPeriodDisplay(int month, int year)
    {
        var date = new DateTime(year, month, 1);
        return date.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
    }

    #endregion
}