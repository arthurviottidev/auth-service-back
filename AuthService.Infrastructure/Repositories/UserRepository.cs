using System.Data;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Dapper;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository(DbConnectionFactory connectionFactory) : IUserRepository
{
    private IDbConnection Conn() => connectionFactory.CreateConnection();

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email", new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = Conn();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE id = @Id", new { Id = id });
    }

    public async Task<bool> ExistsAsync(string email)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM users WHERE email = @Email", new { Email = email });
    }

    public async Task<Guid> CreateAsync(User user)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<Guid>(@"
            INSERT INTO users (id, email, password_hash, full_name, role, is_active, created_at)
            VALUES (@Id, @Email, @PasswordHash, @FullName, @Role, @IsActive, @CreatedAt)
            RETURNING id", user);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(@"
            INSERT INTO refresh_tokens (id, user_id, token, expires_at)
            VALUES (gen_random_uuid(), @UserId, @Token, @ExpiresAt)",
            new { UserId = userId, Token = token, ExpiresAt = expiresAt });
    }

    public async Task<(Guid UserId, bool IsValid)> ValidateRefreshTokenAsync(string token)
    {
        using var conn = Conn();
        var result = await conn.QueryFirstOrDefaultAsync(@"
            SELECT user_id AS UserId, 
                   (revoked_at IS NULL AND expires_at > NOW()) AS IsValid
            FROM refresh_tokens
            WHERE token = @Token",
            new { Token = token });

        if (result is null) return (Guid.Empty, false);
        return (result.UserId, result.IsValid);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(@"
            UPDATE refresh_tokens SET revoked_at = NOW() WHERE token = @Token",
            new { Token = token });
    }

    public async Task SavePasswordResetTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(@"
            INSERT INTO password_reset_tokens (id, user_id, token, expires_at)
            VALUES (gen_random_uuid(), @UserId, @Token, @ExpiresAt)",
            new { UserId = userId, Token = token, ExpiresAt = expiresAt });
    }

    public async Task<(Guid UserId, bool IsValid)> ValidatePasswordResetTokenAsync(string token)
    {
        using var conn = Conn();
        var result = await conn.QueryFirstOrDefaultAsync(@"
            SELECT user_id AS UserId,
                   (used_at IS NULL AND expires_at > NOW()) AS IsValid
            FROM password_reset_tokens
            WHERE token = @Token",
            new { Token = token });

        if (result is null) return (Guid.Empty, false);
        return (result.UserId, result.IsValid);
    }

    public async Task MarkPasswordResetTokenUsedAsync(string token)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(@"
            UPDATE password_reset_tokens SET used_at = NOW() WHERE token = @Token",
            new { Token = token });
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(@"
            UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId",
            new { UserId = userId, PasswordHash = passwordHash });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = Conn();
        return await conn.QueryAsync<User>("SELECT * FROM users ORDER BY created_at DESC");
    }

    public async Task SetActiveStatusAsync(Guid id, bool isActive)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UPDATE users SET is_active = @IsActive WHERE id = @Id",
            new { Id = id, IsActive = isActive });
    }

    public async Task UpdateRoleAsync(Guid id, string role)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "UPDATE users SET role = @Role WHERE id = @Id",
            new { Id = id, Role = role });
    }
}