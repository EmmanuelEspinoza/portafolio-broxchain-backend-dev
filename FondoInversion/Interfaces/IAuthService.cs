using FondoInversion.Models;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateWithSSO(SSOUserData ssoData);
    Task<AuthResponse> RefreshToken(string refreshToken);
    Task<bool> RevokeRefreshToken(string refreshToken);
    Task<user> ValidateToken(string refreshToken);
}