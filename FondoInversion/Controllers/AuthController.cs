using FondoInversion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("sso")]
    public async Task<IActionResult> SSOLogin([FromBody] SSOUserData ssoData, [FromQuery] bool useCookies = false)
    {
        try
        {
            var authResponse = await _authService.AuthenticateWithSSO(ssoData);

            if (useCookies && IsWebRequest(Request))
            {
                // Para web: usar cookies
                SetTokenCookies(authResponse.AccessToken, authResponse.RefreshToken);
                return Ok(new AuthResponse
                {
                    User = authResponse.User,
                    ExpiresAt = authResponse.ExpiresAt
                });
            }
            else
            {
                // Para móvil: devolver tokens en el body
                return Ok(new AuthResponse
                {
                    AccessToken = authResponse.AccessToken,
                    RefreshToken = authResponse.RefreshToken,
                    User = authResponse.User,
                    ExpiresAt = authResponse.ExpiresAt
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error durante la autenticación" });
        }
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, [FromHeader] string authorization = null)
    {
        try
        {
            string refreshToken;

            // Determinar de dónde obtener el refresh token
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            {
                // Para móvil: viene en el header
                refreshToken = authorization.Replace("Bearer ", "");
            }
            else if (Request.Cookies.ContainsKey("refreshToken"))
            {
                // Para web: viene en la cookie
                refreshToken = Request.Cookies["refreshToken"];
            }
            else if (request?.RefreshToken != null)
            {
                // Para móvil: viene en el body (approach alternativo)
                refreshToken = request.RefreshToken;
            }
            else
            {
                return Unauthorized(new { message = "Refresh token no disponible" });
            }

            var authResponse = await _authService.RefreshToken(refreshToken);

            // Determinar cómo devolver la respuesta
            if (IsWebRequest(Request) && Request.Cookies.ContainsKey("refreshToken"))
            {
                SetTokenCookies(authResponse.AccessToken, authResponse.RefreshToken);
                return Ok(new AuthResponse
                {
                    User = authResponse.User,
                    ExpiresAt = authResponse.ExpiresAt
                });
            }
            else
            {
                return Ok(new AuthResponse
                {
                    AccessToken = authResponse.AccessToken,
                    RefreshToken = authResponse.RefreshToken,
                    User = authResponse.User,
                    ExpiresAt = authResponse.ExpiresAt
                });
            }
        }
        catch (SecurityTokenException ex)
        {
            ClearTokenCookies();
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {

            return BadRequest(new { message = "Error durante la renovación del token" });
        }
    }

    [JwtAuthorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var user = await _authService.ValidateToken(request.Token);
            if (user == null)
                return Unauthorized();

            return Ok(new { valid = true, user });
        }
        catch (Exception ex)
        {

            return StatusCode(500, ex.Message); 
        }

    }

    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RevokeRefreshToken(request.RefreshToken);
        if (result)
            return Ok(new { message = "Token revocado exitosamente" });

        return BadRequest(new { message = "Error al revocar el token" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        ClearTokenCookies();
        return await RevokeToken(request);
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // IMPORTANTE: No accesible desde JavaScript
            Secure = true,   // Solo enviar sobre HTTPS en producción
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(30) // Para access token
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        // Opciones para refresh token (más largo)
        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
    }

    private bool IsWebRequest(HttpRequest request)
    {
        // Detectar si es una request de navegador web
        var userAgent = request.Headers["User-Agent"].ToString();
        return userAgent.Contains("Mozilla") ||
                userAgent.Contains("Chrome") ||
                userAgent.Contains("Safari");
    }

}

