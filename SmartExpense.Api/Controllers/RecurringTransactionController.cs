using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartExpense.Application.Dtos.RecurringTransaction;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Constants;

namespace SmartExpense.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = IdentityRoleConstants.User)]
public class RecurringTransactionController : ControllerBase
{
    private readonly IRecurringTransactionService _recurringTransactionService;

    public RecurringTransactionController(IRecurringTransactionService recurringTransactionService)
    {
        _recurringTransactionService = recurringTransactionService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<RecurringTransactionReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RecurringTransactionReadDto>>> GetAll([FromQuery] bool? isActive = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recurring = await _recurringTransactionService.GetAllAsync(userId, isActive);
        return Ok(recurring);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RecurringTransactionReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionReadDto>> GetById(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recurring = await _recurringTransactionService.GetByIdAsync(id, userId);
        return Ok(recurring);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecurringTransactionReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionReadDto>> Create(RecurringTransactionCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recurring = await _recurringTransactionService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = recurring.Id }, recurring);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RecurringTransactionReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionReadDto>> Update(int id, RecurringTransactionUpdateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recurring = await _recurringTransactionService.UpdateAsync(id, dto, userId);
        return Ok(recurring);
    }


    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _recurringTransactionService.DeleteAsync(id, userId);
        return NoContent();
    }


    [HttpPost("{id:int}/toggle")]
    [ProducesResponseType(typeof(RecurringTransactionReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionReadDto>> ToggleActive(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recurring = await _recurringTransactionService.ToggleActiveAsync(id, userId);
        return Ok(recurring);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateTransactionsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GenerateTransactionsResultDto>> GenerateTransactions()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _recurringTransactionService.GenerateTransactionsAsync(userId);
        return Ok(result);
    }


    [HttpPost("{id:int}/generate")]
    [ProducesResponseType(typeof(GenerateTransactionsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateTransactionsResultDto>> GenerateForRecurring(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _recurringTransactionService.GenerateForRecurringTransactionAsync(id, userId);
        return Ok(result);
    }
}