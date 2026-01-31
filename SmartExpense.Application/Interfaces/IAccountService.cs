using SmartExpense.Application.Dtos.Auth;

namespace SmartExpense.Application.Interfaces;

public interface IAccountService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<BasicResponse> LogoutAsync(Guid userId);
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<BasicResponse> DeleteAccountAsync(Guid userId);
}