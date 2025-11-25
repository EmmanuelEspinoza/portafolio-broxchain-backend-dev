using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly EventLogService _eventLogService;
    private readonly AesEncryptionService _aesEncryptionService;

    public AuthController(IAuthService authService, EventLogService eventLogService, AesEncryptionService aesEncryptionSerice)
    {
        _authService = authService;
        _eventLogService = eventLogService;
        _aesEncryptionService = aesEncryptionSerice;
    }

    [HttpPost("sso")]
    public async Task<IActionResult> SSOLogin([FromBody] SSOUserData ssoData, [FromQuery] bool useCookies = false)
    {
        try
        {
            var authResponse = await _authService.AuthenticateWithSSO(ssoData);

            var ecAccesToken = await _aesEncryptionService.EncryptAsync(authResponse.AccessToken);
            var ecRefrhesrToken = await _aesEncryptionService.EncryptAsync(authResponse.RefreshToken);

            if (useCookies && IsWebRequest(Request))
            {
                // Para web: usar cookies
                SetTokenCookies(ecAccesToken, ecRefrhesrToken);
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
                    AccessToken = ecAccesToken,
                    RefreshToken = ecRefrhesrToken,
                    User = authResponse.User,
                    ExpiresAt = authResponse.ExpiresAt
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(ssoData));
            return Unauthorized(new { message = "Error durante la autenticación"});
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(ssoData));
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
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(request));
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            ClearTokenCookies();

            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(request));
            return BadRequest(new { message = "Error durante la renovación del token" });
        }
    }

    [JwtAuthorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var token = await _aesEncryptionService.DecryptAsync(request.Token);
            var user = await _authService.ValidateToken(token);
            if (user == null)
                return Unauthorized();

            return Ok(new { valid = true, user });
        }
        catch (Exception ex)
        {

            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(request));
            return StatusCode(500, "ocurrio un error durante la validación del token"); 
        }

    }

    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var token = await _aesEncryptionService.DecryptAsync(request.RefreshToken);
            var result = await _authService.RevokeRefreshToken(token);
            if (result)
                return Ok(new { message = "Token revocado exitosamente" });

            return BadRequest(new { message = "Error al revocar el token" });
        }
        catch (Exception ex)
        {
            
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(request));
            return BadRequest(new {message = "error al revocar el token "});
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {   
            ClearTokenCookies();
            return await RevokeToken(request);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error,JsonSerializer.Serialize(request));
            return StatusCode(500, "Error al cerrar la sesión");
        }
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

