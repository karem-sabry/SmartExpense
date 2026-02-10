using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartExpense.Application.Dtos.Budget;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Constants;

namespace SmartExpense.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = IdentityRoleConstants.User)]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }


    [HttpGet]
    [ProducesResponseType(typeof(List<BudgetReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BudgetReadDto>>> GetAll(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var budgets = await _budgetService.GetAllAsync(userId, month, year);
        return Ok(budgets);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetReadDto>> GetById(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var budget = await _budgetService.GetByIdAsync(id, userId);
        return Ok(budget);
    }


    [HttpGet("summary")]
    [ProducesResponseType(typeof(BudgetSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var summary = await _budgetService.GetSummaryAsync(userId, month, year);
        return Ok(summary);
    }


    [HttpPost]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BudgetReadDto>> Create(BudgetCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var budget = await _budgetService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetReadDto>> Update(int id, BudgetUpdateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var budget = await _budgetService.UpdateAsync(id, dto, userId);
        return Ok(budget);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _budgetService.DeleteAsync(id, userId);
        return NoContent();
    }
}