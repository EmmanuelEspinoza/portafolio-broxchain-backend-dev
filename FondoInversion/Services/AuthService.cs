using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FondoInversion.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger, TokenService tokenService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> AuthenticateWithSSO(SSOUserData ssoData)
    {
        try
        {
            var userData = await _userRepository.GetUserByEmailAsync(ssoData.email);
            var expires =  DateTime.UtcNow.AddMinutes(30);

            if (userData == null)
            {
                _logger.LogWarning($"Intento de autenticación fallido para email: {ssoData.email}");
                throw new UnauthorizedAccessException("Usuario no registrado en el sistema");
            }

            var token = _tokenService.GenerateAccessToken(userData);
            var refreshToken = _tokenService.GenerateRefreshToken(userData);
            
            var userResp = new userResponse
            {
                name = userData.name,
                correo = userData.correo
            };

            await _tokenService.SaveRefreshToken(refreshToken, userData, expires, 1);
            
            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = expires,
                User = userResp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante autenticación SSO");
            throw;
        }
    }


    public async Task<AuthResponse> RefreshToken(string refreshToken)
    {
        return await _tokenService.RefreshToken(refreshToken);
    }

    public Task<bool> RevokeRefreshToken(string refreshToken)
    {
        return _tokenService.RevokeRefreshToken(refreshToken);
    }

    public Task<user> ValidateToken(string refrehToken)
    {
        return _tokenService.ValidateToken(refrehToken);
    }
}