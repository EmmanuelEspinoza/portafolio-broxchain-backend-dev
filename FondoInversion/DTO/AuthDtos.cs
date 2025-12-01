using System.ComponentModel.DataAnnotations;

public class SSOUserData
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")] 
    public string email { get; set; }
    
    [Required(ErrorMessage = "El password es obligatorio")]
    public string password { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public userResponse User { get; set; }
    public string RefreshToken { get; set; }
}

// DTOs para las requests
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}

public class ValidateTokenRequest
{
    public string Token { get; set; }
}