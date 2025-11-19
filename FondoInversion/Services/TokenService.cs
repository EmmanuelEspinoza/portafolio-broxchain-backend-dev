
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FondoInversion.Models;
using Microsoft.IdentityModel.Tokens;

public class TokenService
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IUserRepository _userRepository;

    public TokenService(IConfiguration configuration, ITokenRepository tokenRepository, IUserRepository userRepository, ILogger<TokenService> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse> RefreshToken(string refreshToken)
    {
        try
        {
            var expires = DateTime.UtcNow.AddDays(7);
            var userRefreshToken = await _tokenRepository.GetDataByRefreshToken(refreshToken);

            if(userRefreshToken == null)
            {
                throw new SecurityTokenException("Refresh token no existe"); 
            }

            if (userRefreshToken != null && userRefreshToken.activo == 0)
            {
                throw new SecurityTokenException("Refresh token revocado");
            }

            // Validar el refresh token y obtener el usuario
            var user = await ValidateRefreshToken(refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Intento de refresh con token inválido");
                throw new SecurityTokenException("Refresh token inválido o expirado");
            }

            // Generar nuevos tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            // En producción, actualizar el refresh token en la base de datos
            // await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken, newRefreshToken);
            var userResp = new userResponse
            {
                name = user.name,
                correo = user.correo
            };

            await UpdateRefreshToken(newRefreshToken,userRefreshToken, expires, 1);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                User = userResp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante refresh token");
            throw;
        }
    }

    public async Task<user> ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Verificar que NO sea un refresh token
            var tokenType = jwtToken.Claims.FirstOrDefault(x => x.Type == "tokenType")?.Value;
            if (tokenType == "refresh")
            {
                throw new SecurityTokenException("Refresh token no válido para autorización");
            }

            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "nameid").Value);
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token expirado");
            throw; // Relanzar para que el filter lo capture
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token de seguridad inválido");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando token");
            throw new SecurityTokenException("Token inválido", ex);
        }
    }

    public async Task<bool> RevokeRefreshToken(string refreshToken)
    {
        try
        {
            // Validar el token primero
            var user = await ValidateRefreshToken(refreshToken);
            
            var userRefreshToken = await _tokenRepository.GetDataByRefreshToken(refreshToken);
            if (userRefreshToken != null)
            {
                var expires = DateTime.UtcNow;
                await UpdateRefreshToken(refreshToken, userRefreshToken, expires, 0);
                // En producción: await _userRepository.RevokeRefreshTokenAsync(user.Id, refreshToken);
                _logger.LogInformation($"Refresh token revocado para usuario: {user.correo}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revocando refresh token");
            return false;
        }
    }

    public string GenerateAccessToken(user user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier , user.id.ToString()),
                new Claim(ClaimTypes.Email, user.correo),
                new Claim(ClaimTypes.Name, user.name ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken(user user)
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshSecret"] ?? _configuration["Jwt:Secret"]);
        var expires = DateTime.UtcNow.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim("tokenType", "refresh"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = expires,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = tokenHandler.WriteToken(token);

        return refreshToken;
    }

    private async Task<user> ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshSecret"] ?? _configuration["Jwt:Secret"]);

            tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Verificar que sea un refresh token
            var tokenType = jwtToken.Claims.FirstOrDefault(x => x.Type == "tokenType")?.Value;
            if (tokenType != "refresh")
            {
                throw new SecurityTokenException("Token no es un refresh token válido");
            }

            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "nameid").Value);
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando refresh token");
            return null;
        }
    }

    public async Task<bool> SaveRefreshToken(string newRefreshToken, user userData, DateTime expires, int active)
    {
        var userToken = new token
        {
            userID = userData.id,
            refreshToken = newRefreshToken,
            expiracion = expires,
            activo = active
        };
        await _tokenRepository.AddAsync(userToken);
        await _tokenRepository.SaveAsync();
        return true;
    }

    public async Task<bool> UpdateRefreshToken(string RefreshToken, token tokenData, DateTime expires, int active)
    {
        tokenData.refreshToken = RefreshToken;
        tokenData.expiracion = expires;
        tokenData.activo = active;
        _tokenRepository.Update(tokenData);
        await _tokenRepository.SaveAsync();
        return true;
    }

}




