using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

[Table("accesoUser")]
public partial class accesoUser
{
    [Key]
    public int id { get; set; }

    public int userID { get; set; }

    public int intentos { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ultimoIntento { get; set; }

    [ForeignKey("userID")]
    [InverseProperty("accesoUsers")]
    public virtual user user { get; set; } = null!;
}
