
using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(500, ErrorMessage = "El nombre no puede exceder 500 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
    public string Correo { get; set; }

    [Required(ErrorMessage = "El RFC es obligatoria")]
    [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
    [DataType(DataType.Password)]
    public string Rfc { get; set; }

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20)]
    public string Telefono { get; set; }

    [Required(ErrorMessage = "El CURP es obligatoria")]
    [DataType(DataType.Password)]
    public string Curp { get; set; }

}


public class EditUserDto
{
    public int Id { get; set; } // Necesario para identificar el usuario a editar

    [StringLength(500, ErrorMessage = "El nombre no puede exceder 500 caracteres")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
    public string? Correo { get; set; }

    [StringLength(13, MinimumLength = 12, ErrorMessage = "El Rfc debe tener entre 12 y 13 caracteres")]
    public string? Rfc { get; set; }

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [DataType(DataType.Password)]
    public string? Curp { get; set; }

    public bool IsActive { get; set; } // Para activar/desactivar usuario
}


public class userResponse
{
    public string name {get; set;}
    public string correo {get; set;}
}