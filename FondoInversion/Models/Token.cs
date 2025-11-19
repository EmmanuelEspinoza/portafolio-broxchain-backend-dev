using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

public partial class token
{
    [Key]
    public int id { get; set; }

    public int userID { get; set; }

    [Unicode(false)]
    public string refreshToken { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime expiracion { get; set; }

    public int activo { get; set; }

    [ForeignKey("userID")]
    [InverseProperty("tokens")]
    public virtual user user { get; set; } = null!;
}
