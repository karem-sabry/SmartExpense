using System.Globalization;
using SmartExpense.Application.Dtos.Analytics;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Enums;
using SmartExpense.Core.Models;

namespace SmartExpense.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialOverviewDto> GetFinancialOverviewAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        var totalIncome = await _unitOfWork.Transactions.GetTotalIncomeAsync(userId, startDate, endDate);
        var totalExpense = await _unitOfWork.Transactions.GetTotalExpenseAsync(userId, startDate, endDate);
        var transactionCount = await _unitOfWork.Transactions.GetTransactionCountAsync(userId, startDate, endDate);

        var allTransactions = await _unitOfWork.Transactions.GetPagedAsync(userId, new TransactionQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            PageSize = int.MaxValue
        });

        var incomeCount = allTransactions.Data.Count(t => t.TransactionType == TransactionType.Income);
        var expenseCount = allTransactions.Data.Count(t => t.TransactionType == TransactionType.Expense);

        var days = (endDate - startDate).Days + 1;
        var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpense) / totalIncome) * 100 : 0;

        var topExpenseCategories = await GetTopCategoriesAsync(userId, startDate, endDate, 5, expenseOnly: true);
        var topIncomeCategories = await GetTopCategoriesAsync(userId, startDate, endDate, 5, expenseOnly: false);

        var dailyTrend = await GetSpendingTrendsAsync(userId, startDate, endDate, "daily");

        return new FinancialOverviewDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            SavingsRate = Math.Round(savingsRate, 2),
            AverageDailyIncome = days > 0 ? Math.Round(totalIncome / days, 2) : 0,
            AverageDailyExpense = days > 0 ? Math.Round(totalExpense / days, 2) : 0,
            TotalTransactions = transactionCount,
            IncomeTransactionCount = incomeCount,
            ExpenseTransactionCount = expenseCount,
            TopExpenseCategories = topExpenseCategories,
            TopIncomeCategories = topIncomeCategories,
            DailyTrend = dailyTrend
        };
    }

    public async Task<List<SpendingTrendDto>> GetSpendingTrendsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string groupBy = "monthly")
    {
        var allTransactions = await _unitOfWork.Transactions.GetPagedAsync(userId, new TransactionQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            PageSize = int.MaxValue
        });

        var trends = groupBy.ToLower() switch
        {
            "daily" => GroupByDay(allTransactions.Data, startDate, endDate),
            "weekly" => GroupByWeek(allTransactions.Data, startDate, endDate),
            "monthly" => GroupByMonth(allTransactions.Data, startDate, endDate),
            _ => GroupByMonth(allTransactions.Data, startDate, endDate)
        };

        return trends;
    }

    public async Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        bool expenseOnly = true)
    {
        var transactionType = expenseOnly ? TransactionType.Expense : TransactionType.Income;

        var transactions = await _unitOfWork.Transactions.GetPagedAsync(userId, new TransactionQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            TransactionType = transactionType,
            PageSize = int.MaxValue
        });

        var total = transactions.Data.Sum(t => t.Amount);

        var breakdown = transactions.Data
            .GroupBy(t => new { t.CategoryId, t.Category?.Name, t.Category?.Icon, t.Category?.Color })
            .Select(g => new CategoryBreakdownDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name ?? "Unknown",
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                TotalAmount = g.Sum(t => t.Amount),
                Percentage = total > 0 ? Math.Round((g.Sum(t => t.Amount) / total) * 100, 2) : 0,
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return breakdown;
    }

    public async Task<List<MonthlyComparisonDto>> GetMonthlyComparisonAsync(
        Guid userId,
        int numberOfMonths = 6)
    {
        var comparisons = new List<MonthlyComparisonDto>();
        var currentDate = DateTime.UtcNow;

        for (int i = numberOfMonths - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var startDate = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var income = await _unitOfWork.Transactions.GetTotalIncomeAsync(userId, startDate, endDate);
            var expense = await _unitOfWork.Transactions.GetTotalExpenseAsync(userId, startDate, endDate);
            var count = await _unitOfWork.Transactions.GetTransactionCountAsync(userId, startDate, endDate);

            comparisons.Add(new MonthlyComparisonDto
            {
                Month = targetDate.Month,
                Year = targetDate.Year,
                Period = targetDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                TotalIncome = income,
                TotalExpense = expense,
                NetBalance = income - expense,
                TransactionCount = count
            });
        }

        // Calculate month-over-month changes
        for (int i = 1; i < comparisons.Count; i++)
        {
            var current = comparisons[i];
            var previous = comparisons[i - 1];

            current.IncomeChange = previous.TotalIncome > 0
                ? Math.Round(((current.TotalIncome - previous.TotalIncome) / previous.TotalIncome) * 100, 2)
                : 0;

            current.ExpenseChange = previous.TotalExpense > 0
                ? Math.Round(((current.TotalExpense - previous.TotalExpense) / previous.TotalExpense) * 100, 2)
                : 0;
        }

        return comparisons;
    }

    public async Task<List<BudgetPerformanceDto>> GetBudgetPerformanceAsync(
        Guid userId,
        int month,
        int year)
    {
        var budgets = await _unitOfWork.Budgets.GetByMonthYearAsync(userId, month, year);
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var performance = new List<BudgetPerformanceDto>();

        foreach (var budget in budgets)
        {
            var transactions = await _unitOfWork.Transactions.GetPagedAsync(userId, new TransactionQueryParameters
            {
                CategoryId = budget.CategoryId,
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = TransactionType.Expense,
                PageSize = int.MaxValue
            });

            var actualSpent = transactions.Data.Sum(t => t.Amount);
            var percentageUsed = budget.Amount > 0 ? (actualSpent / budget.Amount) * 100 : 0;

            var status = percentageUsed >= 100 ? "Exceeded" :
                        percentageUsed >= 80 ? "Approaching" : "Under";

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var daysPassed = DateTime.UtcNow.Day;
            var expectedPercentage = (daysPassed / (double)daysInMonth) * 100;
            var isOnTrack = percentageUsed <= (decimal)expectedPercentage;

            performance.Add(new BudgetPerformanceDto
            {
                BudgetId = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.Name ?? "Unknown",
                CategoryIcon = budget.Category?.Icon,
                BudgetAmount = budget.Amount,
                ActualSpent = actualSpent,
                Remaining = budget.Amount - actualSpent,
                PercentageUsed = Math.Round(percentageUsed, 2),
                Status = status,
                IsOnTrack = isOnTrack
            });
        }

        return performance.OrderByDescending(p => p.PercentageUsed).ToList();
    }

    public async Task<List<TopCategoryDto>> GetTopCategoriesAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        int count = 5,
        bool expenseOnly = true)
    {
        var transactionType = expenseOnly ? TransactionType.Expense : TransactionType.Income;

        var transactions = await _unitOfWork.Transactions.GetPagedAsync(userId, new TransactionQueryParameters
        {
            StartDate = startDate,
            EndDate = endDate,
            TransactionType = transactionType,
            PageSize = int.MaxValue
        });

        var topCategories = transactions.Data
            .GroupBy(t => new { t.CategoryId, t.Category?.Name, t.Category?.Icon, t.Category?.Color })
            .Select(g => new TopCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name ?? "Unknown",
                CategoryIcon = g.Key.Icon,
                CategoryColor = g.Key.Color,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                AverageTransaction = g.Average(t => t.Amount)
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(count)
            .ToList();

        return topCategories;
    }

    #region Private Helper Methods

    private static List<SpendingTrendDto> GroupByDay(
        List<Core.Entities.Transaction> transactions,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<SpendingTrendDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            var dayTransactions = transactions.Where(t => t.TransactionDate.Date == currentDate).ToList();

            trends.Add(new SpendingTrendDto
            {
                Date = currentDate,
                Period = currentDate.ToString("MMM dd", CultureInfo.InvariantCulture),
                TotalIncome = dayTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense = dayTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                NetBalance = dayTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) -
                           dayTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = dayTransactions.Count
            });

            currentDate = currentDate.AddDays(1);
        }

        return trends;
    }

    private static List<SpendingTrendDto> GroupByWeek(
        List<Core.Entities.Transaction> transactions,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<SpendingTrendDto>();
        var currentDate = startDate.Date;
        var weekNumber = 1;

        while (currentDate <= endDate)
        {
            var weekEnd = currentDate.AddDays(6);
            if (weekEnd > endDate) weekEnd = endDate;

            var weekTransactions = transactions
                .Where(t => t.TransactionDate.Date >= currentDate && t.TransactionDate.Date <= weekEnd)
                .ToList();

            trends.Add(new SpendingTrendDto
            {
                Date = currentDate,
                Period = $"Week {weekNumber}",
                TotalIncome = weekTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense = weekTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                NetBalance = weekTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) -
                           weekTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = weekTransactions.Count
            });

            currentDate = weekEnd.AddDays(1);
            weekNumber++;
        }

        return trends;
    }

    private static List<SpendingTrendDto> GroupByMonth(
        List<Core.Entities.Transaction> transactions,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<SpendingTrendDto>();
        var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

        while (currentDate <= endDate)
        {
            var monthEnd = currentDate.AddMonths(1).AddDays(-1);
            if (monthEnd > endDate) monthEnd = endDate;

            var monthTransactions = transactions
                .Where(t => t.TransactionDate >= currentDate && t.TransactionDate <= monthEnd)
                .ToList();

            trends.Add(new SpendingTrendDto
            {
                Date = currentDate,
                Period = currentDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                TotalIncome = monthTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense = monthTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                NetBalance = monthTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount) -
                           monthTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = monthTransactions.Count
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    #endregion
}