using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

[Table("eventLog")]
public partial class eventLog
{
    [Key]
    public int id { get; set; }

    public int type { get; set; }

    [Unicode(false)]
    public string message { get; set; } = null!;

    [Column(TypeName = "text")]
    public string body { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime fecha { get; set; }
}
