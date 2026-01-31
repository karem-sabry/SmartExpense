using SmartExpense.Core.Entities;

namespace SmartExpense.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

}