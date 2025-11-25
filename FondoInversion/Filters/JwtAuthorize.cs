using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using FondoInversion.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public class JwtAuthorizeAttribute : Attribute, IAsyncActionFilter
{

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            string token = null;
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            var asEncryptService = httpContext.RequestServices.GetRequiredService<IEncryptionService>();


            // 1. Intentar obtener el token del Header (para móvil)
            if (request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authHeader = authorizationHeader.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            // 2. Si no hay token en header, intentar obtener de Cookie (para web)
            if (string.IsNullOrEmpty(token) && request.Cookies.TryGetValue("accessToken", out var cookieToken))
            {
                token = cookieToken;
            }

            // 3. Validar que se obtuvo un token
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Token de autorización requerido",
                    error = "missing_token"
                });
                return;
            }

            // 3.5  Desencriptar token 
            token = await asEncryptService.DecryptAsync(token);

            // 4. Verificar que no sea un refresh token
            if (IsRefreshToken(token))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Refresh token no puede ser usado para autorización",
                    error = "invalid_token_type"
                });
                return;
            }

            // 5. Validar token
            var authService = httpContext.RequestServices.GetRequiredService<IAuthService>();
            var user = await authService.ValidateToken(token);

            if (user == null)
            {

                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Token inválido o expirado",
                    error = "invalid_token"
                });
                return;
            }

            // 6. Verificar si el usuario está activo
            if (!IsUserActive(user))
            {
                context.Result = new ObjectResult(new
                {
                    message = "Usuario inactivo",
                    error = "user_inactive"
                })
                { StatusCode = 403 };
                return;
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Email, user.correo),
                new Claim("AuthToken", token),
                new Claim("AuthSource", GetTokenSource(request, token))
            };

            var identity = new ClaimsIdentity(claims, "Custom");
            var principal = new ClaimsPrincipal(identity);

            httpContext.User = principal;

            // 7. Agregar información al contexto
            httpContext.Items["User"] = user;
            httpContext.Items["UserId"] = user.id;
            httpContext.Items["UserEmail"] = user.correo;
            httpContext.Items["AuthToken"] = token;
            httpContext.Items["AuthSource"] = GetTokenSource(request, token);
            httpContext.Items["NameIdentifier"] = user.id;


            await next();
        }
        catch (SecurityTokenExpiredException ex)
        {
            // Token expirado - sugerir refresh
            context.Result = new ObjectResult(new
            {
                message = "Token expirado",
                error = "token_expired",
                shouldRefresh = true
            })
            { StatusCode = 401 };
        }
        catch (SecurityTokenException ex)
        {
            // Error específico de token
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Token de seguridad inválido",
                error = "invalid_token"
            });
        }
        catch (Exception ex)
        {
            // Log the exception
            var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtAuthorizeAttribute>>();
            logger?.LogError(ex, "Error en autorización JWT");

            context.Result = new ObjectResult(new
            {
                message = "Error de autorización",
                error = "authorization_error"
            })
            { StatusCode = 500 };
        }
    }

    private bool IsRefreshToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return false;

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;

            return tokenType == "refresh";
        }
        catch
        {
            return false;
        }
    }

    private bool IsUserActive(user user)
    {
        // Implementa tu lógica de verificación de usuario activo
        // Por ejemplo:
        // return user.Estado == "Activo";
        // return user.Activo == true;

        // Por ahora, asumimos que todos los usuarios están activos
        return true;
    }

    private string GetTokenSource(HttpRequest request, string token)
    {
        // Determinar la fuente del token para logging/debugging
        if (request.Headers.TryGetValue("Authorization", out var authHeader) &&
            authHeader.ToString().Contains(token))
        {
            return "header";
        }

        if (request.Cookies.TryGetValue("accessToken", out var cookieToken) &&
            cookieToken == token)
        {
            return "cookie";
        }

        return "unknown";
    }
}

// Versión opcional que permite endpoints públicos pero still valida el token si está presente
public class JwtOptionalAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            string token = null;
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            // Intentar obtener token de header
            if (request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authHeader = authorizationHeader.ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            // Intentar obtener token de cookie
            if (string.IsNullOrEmpty(token) && request.Cookies.TryGetValue("accessToken", out var cookieToken))
            {
                token = cookieToken;
            }

            // Si hay token, validarlo
            if (!string.IsNullOrEmpty(token) && !IsRefreshToken(token))
            {
                var authService = httpContext.RequestServices.GetRequiredService<IAuthService>();
                var user = await authService.ValidateToken(token);

                if (user != null && IsUserActive(user))
                {
                    httpContext.Items["User"] = user;
                    httpContext.Items["UserId"] = user.id;
                    httpContext.Items["UserEmail"] = user.correo;
                    httpContext.Items["AuthToken"] = token;
                    httpContext.Items["AuthSource"] = GetTokenSource(request, token);
                    httpContext.Items["IsAuthenticated"] = true;
                }
            }
            else
            {
                httpContext.Items["IsAuthenticated"] = false;
            }

            await next();
        }
        catch (Exception ex)
        {
            // En modo opcional, no bloqueamos la request por errores de token
            var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtOptionalAuthorizeAttribute>>();
            logger?.LogError(ex, "Error en autorización JWT opcional");

            context.HttpContext.Items["IsAuthenticated"] = false;
            await next();
        }
    }

    private bool IsRefreshToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return false;

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "tokenType")?.Value;

            return tokenType == "refresh";
        }
        catch
        {
            return false;
        }
    }

    private bool IsUserActive(user user)
    {
        return true; // Implementa tu lógica
    }

    private string GetTokenSource(HttpRequest request, string token)
    {
        if (request.Headers.TryGetValue("Authorization", out var authHeader) &&
            authHeader.ToString().Contains(token))
        {
            return "header";
        }

        if (request.Cookies.TryGetValue("accessToken", out var cookieToken) &&
            cookieToken == token)
        {
            return "cookie";
        }

        return "unknown";
    }
}