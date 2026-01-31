using System.ComponentModel.DataAnnotations;

namespace SmartExpense.Application.Dtos.Auth;

public record RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string? RefreshToken { get; init; }
}