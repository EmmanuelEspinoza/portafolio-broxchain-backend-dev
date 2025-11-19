using System;
using System.Collections.Generic;

namespace FondoInversion.Models;

public partial class VistaPreciosFlujosUsuario
{
    public DateTime? FechaPrecio { get; set; }

    public double? PrecioMxn { get; set; }

    public int? InversionistaId { get; set; }

    public double? Importe { get; set; }

    public double? Comision { get; set; }
}
