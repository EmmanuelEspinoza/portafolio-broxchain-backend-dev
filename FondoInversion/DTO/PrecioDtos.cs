
using System.ComponentModel.DataAnnotations;

public class CreatePrecioDto
{
    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateOnly FechaPrecio { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    public double PrecioMxn { get; set; }

}


public class EditPrecioDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateOnly FechaPrecio { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    public double PrecioMxn { get; set; }

    public bool? IsActive { get; set; }
}

public class GetSaldoDto
{
    public TipoSaldo tipo { get; set; }
}

public enum TipoSaldo
{
    General,
    Usuario
}


public class SaldosDtos
{
    public string titulo { get; set; }
    public string textoSecundario { get; set; }
    public decimal valorPrincipal { get; set; }
    public decimal? valorSecundario { get; set; }
    public decimal? valorTerciario { get; set; }
    public string color { get; set; }
    public string? tooltip { get; set; }

}

public class ValoresMercado
{
    public decimal montoCirculacion { get; set; }
    public decimal valorMercado { get; set; }
}
