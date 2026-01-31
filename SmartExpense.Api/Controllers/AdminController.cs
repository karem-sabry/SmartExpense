using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartExpense.Application.Dtos.Auth;
using SmartExpense.Application.Interfaces;
using SmartExpense.Core.Constants;

namespace SmartExpense.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = IdentityRoleConstants.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserWithRolesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(UserWithRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _adminService.GetUserByIdAsync(userId);
        return Ok(user);
    }


    [HttpPost("users/{userId:guid}/make-admin")]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeUserAdmin(Guid userId)
    {
        var currentAdminEmail = User.FindFirstValue(ClaimTypes.Email)!;
        var response = await _adminService.MakeUserAdminAsync(userId, currentAdminEmail);
        return Ok(response);
    }

    [HttpPost("users/{userId:guid}/remove-admin")]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAdminRole(Guid userId)
    {
        var currentAdminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _adminService.RemoveAdminRoleAsync(userId, currentAdminId);
        return Ok(response);
    }

    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var currentAdminEmail = User.FindFirstValue(ClaimTypes.Email)!;
        var response = await _adminService.DeleteUserAsync(userId, currentAdminEmail);
        return Ok(response);
    }
}