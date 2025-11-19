using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

[Table("flujo")]
public partial class flujo
{
    [Key]
    public int id { get; set; }

    public int inversionista_id { get; set; }

    public DateOnly dia_movimiento { get; set; }

    public double importe { get; set; }

    public double comision { get; set; }

    [ForeignKey("inversionista_id")]
    [InverseProperty("flujos")]
    public virtual user inversionista { get; set; } = null!;
}
