using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(string email);
    Task<Guid> CreateAsync(User user);
    Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt);
    Task<(Guid UserId, bool IsValid)> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task SavePasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt);
    Task<(Guid UserId, bool IsValid)> ValidatePasswordResetTokenAsync(string token);
    Task MarkPasswordResetTokenUsedAsync(string token);
    Task UpdatePasswordAsync(Guid userId, string passwordHash);
    Task<IEnumerable<User>> GetAllAsync();
    Task SetActiveStatusAsync(Guid id, bool isActive);
    Task UpdateRoleAsync(Guid id, string role);
}