
using System.ComponentModel.DataAnnotations;

public class CreateFlujoDto
{
    [Required(ErrorMessage = "El inversionista es obligatoria")]
    public int InversionistaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatorio")]
    public DateOnly DiaMovimiento { get; set; }

    [Required(ErrorMessage = "El importe es obligatorio")]
    public double Importe { get; set; }

    [Required(ErrorMessage = "La comisión es obligatorio")]
    public double Comision { get; set; }

    // [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
    // public string Tipo { get; set; }

    // [Required(ErrorMessage = "El estatus de la transacción es obligatorio")]
    // public string Estatus { get; set; }
}


public class EditFlujoDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El inversionista es obligatoria")]
    public required int InversionistaId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatorio")]
    public DateOnly DiaMovimiento { get; set; }

    [Required(ErrorMessage = "El importe es obligatorio")]
    public double Importe { get; set; }

    [Required(ErrorMessage = "La comisión es obligatorio")]
    public double Comision { get; set; }

    // [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
    // public string Tipo { get; set; }

    // [Required(ErrorMessage = "El estatus de la transacción es obligatorio")]
    // public required string Estatus { get; set; }
}

public class FlujoPrecio
{
    public DateOnly fecha { get; set; }
    public string tipoMovimiento { get; set; }
    public decimal movimientoUnidad { get; set; }
    public decimal movimientoMxn { get; set; }
    public decimal precio { get; set; }
    public decimal saldoUnidad { get; set; }
    public decimal saldoMxn { get; set; }
}