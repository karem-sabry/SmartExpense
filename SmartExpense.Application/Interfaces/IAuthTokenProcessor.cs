using SmartExpense.Core.Entities;

namespace SmartExpense.Application.Interfaces;

public interface IAuthTokenProcessor
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}