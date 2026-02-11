using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartExpense.Application.Dtos.Analytics;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Constants;

namespace SmartExpense.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = IdentityRoleConstants.User)]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(FinancialOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinancialOverviewDto>> GetFinancialOverview(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var overview = await _analyticsService.GetFinancialOverviewAsync(userId, startDate, endDate);
        return Ok(overview);
    }

    [HttpGet("spending-trends")]
    [ProducesResponseType(typeof(List<SpendingTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SpendingTrendDto>>> GetSpendingTrends(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string groupBy = "monthly")
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var trends = await _analyticsService.GetSpendingTrendsAsync(userId, startDate, endDate, groupBy);
        return Ok(trends);
    }

    [HttpGet("category-breakdown")]
    [ProducesResponseType(typeof(List<CategoryBreakdownDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<CategoryBreakdownDto>>> GetCategoryBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] bool expenseOnly = true)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var breakdown = await _analyticsService.GetCategoryBreakdownAsync(userId, startDate, endDate, expenseOnly);
        return Ok(breakdown);
    }

    [HttpGet("monthly-comparison")]
    [ProducesResponseType(typeof(List<MonthlyComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<MonthlyComparisonDto>>> GetMonthlyComparison(
        [FromQuery] int numberOfMonths = 6)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comparison = await _analyticsService.GetMonthlyComparisonAsync(userId, numberOfMonths);
        return Ok(comparison);
    }

    [HttpGet("budget-performance")]
    [ProducesResponseType(typeof(List<BudgetPerformanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BudgetPerformanceDto>>> GetBudgetPerformance(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var performance = await _analyticsService.GetBudgetPerformanceAsync(userId, month, year);
        return Ok(performance);
    }

    [HttpGet("top-categories")]
    [ProducesResponseType(typeof(List<TopCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TopCategoryDto>>> GetTopCategories(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int count = 5,
        [FromQuery] bool expenseOnly = true)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var topCategories =
            await _analyticsService.GetTopCategoriesAsync(userId, startDate, endDate, count, expenseOnly);
        return Ok(topCategories);
    }
}