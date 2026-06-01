using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IEmailService emailService) : IAuthService
{
    public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await userRepository.ExistsAsync(dto.Email))
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = BC.HashPassword(dto.Password),
            Role = "user",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.CreateAsync(user);
        return await GenerateTokenResponse(user);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!BC.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Usuário inativo.");

        return await GenerateTokenResponse(user);
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var (userId, isValid) = await userRepository.ValidateRefreshTokenAsync(refreshToken);

        if (!isValid)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        await userRepository.RevokeRefreshTokenAsync(refreshToken);

        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        return await GenerateTokenResponse(user);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null) return; // se e-mail é existente ele não é revelado

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(2);

        await userRepository.SavePasswordResetTokenAsync(user.Id, token, expiresAt);
        await emailService.SendPasswordResetEmailAsync(email, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var (userId, isValid) = await userRepository.ValidatePasswordResetTokenAsync(dto.Token);

        if (!isValid)
            throw new InvalidOperationException("Token inválido ou expirado.");

        var passwordHash = BC.HashPassword(dto.NewPassword);
        await userRepository.UpdatePasswordAsync(userId, passwordHash);
        await userRepository.MarkPasswordResetTokenUsedAsync(dto.Token);
    }

    private async Task<TokenResponseDto> GenerateTokenResponse(User user)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        await userRepository.SaveRefreshTokenAsync(user.Id, refreshToken, expiresAt);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}