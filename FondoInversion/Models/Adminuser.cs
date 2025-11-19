using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

public partial class adminUser
{
    [Key]
    public int id { get; set; }

    [Unicode(false)]
    public string nombre { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string correo { get; set; } = null!;

    [Unicode(false)]
    public string password { get; set; } = null!;

    [Unicode(false)]
    public string rol { get; set; } = null!;
}
