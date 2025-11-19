using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

[Table("precio")]
[Index("fecha_precio", Name = "UQ__precio__E5CD410C546B8B96", IsUnique = true)]
public partial class precio
{
    [Key]
    public int id { get; set; }

    public DateOnly fecha_precio { get; set; }

    public double precio_mxn { get; set; }
}
