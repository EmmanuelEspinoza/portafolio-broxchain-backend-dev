using System.ComponentModel;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;


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

    [EndpointSummary("Endpoint de logeo")]
    [EndpointDescription("Servicio que válida si el usuario existe, al comprobarlo genera los tokens para las demás operaciones, para web genera cookies con los tokens y para móviles se envía en la información del JSON de respuesta")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status400BadRequest, "application/json")]
    [HttpPost("sso")]
    public async Task<IActionResult> SSOLogin(
        [Description("Modelo de datos para el login")][FromBody] SSOUserData ssoData, 
        [Description("Indicador de si se desea usar cookies para los tokens")][FromQuery] bool useCookies = false)
    {
        try
        {
            var authResponse = await _authService.AuthenticateWithSSO(ssoData);

            var ecAccesToken = await _aesEncryptionService.EncryptAsync(authResponse.AccessToken);
            var ecRefrhesrToken = await _aesEncryptionService.EncryptAsync(authResponse.RefreshToken);
            var isWebReq =  IsWebRequest(Request);

            if (useCookies || isWebReq)
            {
                SetTokenCookies(ecAccesToken, ecRefrhesrToken);
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
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(ssoData));
            return Unauthorized(new { message = "Error durante la autenticación" });
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(ssoData));
            return BadRequest(new { message = "Error durante la autenticación" });
        }
    }


    [EndpointSummary("Actualizar token de acceso")]
    [EndpointDescription("Servicio que genera un nuevo accesstoken a partir de un refresh token, también este servicio lo valida y en caso de no ser correcto no lo regenera.")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status400BadRequest, "application/json")]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
        [Description("Refresh token del cual se ocupa generar un access token")][FromBody] RefreshTokenRequest request, [FromHeader] string authorization = null)
    {
        try
        {
            string refreshToken;

            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            {
                refreshToken = authorization.Replace("Bearer ", "");
            }
            else if (Request.Cookies.ContainsKey("refreshToken"))
            {
                refreshToken = Request.Cookies["refreshToken"];
            }
            else if (request?.RefreshToken != null)
            {
                refreshToken = request.RefreshToken;
            }
            else
            {
                return Unauthorized(new { message = "Refresh token no disponible" });
            }

            var authResponse = await _authService.RefreshToken(refreshToken);

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
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(request));
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            ClearTokenCookies();

            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(request));
            return BadRequest(new { message = "Error durante la renovación del token" });
        }
    }


    [EndpointSummary("Validar token de acceso")]
    [EndpointDescription("Servicio para validar si el access token sigue siendo válido.")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status401Unauthorized, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status400BadRequest, "application/json")]
    [JwtAuthorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate(
        [Description("Access token a validar ")][FromBody] ValidateTokenRequest request)
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
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(request));
            return StatusCode(500, "ocurrio un error durante la validación del token");
        }

    }


    [EndpointSummary("Revocar permisos del token")]
    [EndpointDescription("Servicio que sirve para marcar invalidar el refresh token .")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status400BadRequest, "application/json")]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken(
        [Description("Token a invalidar")][FromBody] RefreshTokenRequest request)
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
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(request));
            return BadRequest(new { message = "error al revocar el token " });
        }
    }


    [EndpointSummary("Servicio de cierre de sesión.")]
    [EndpointDescription("Este servicio se dedica a limpiar las cookies generadas e invalida el refresh token.")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<AnyType>(StatusCodes.Status400BadRequest, "application/json")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [Description("Refresh token a invalidar")][FromBody] RefreshTokenRequest request)
    {
        try
        {
            ClearTokenCookies();
            return await RevokeToken(request);
        }
        catch (Exception ex)
        {
            var exception = ex.Message + " ---StackTrace--- " + ex.StackTrace;
            await _eventLogService.SaveEventLog(exception, ETipoMessage.Error, JsonSerializer.Serialize(request));
            return StatusCode(500, "Error al cerrar la sesión");
        }
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(30)
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

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
        var userAgent = request.Headers["User-Agent"].ToString();
        return userAgent.Contains("Mozilla") ||
                userAgent.Contains("Chrome") ||
                userAgent.Contains("Safari");
    }

}

