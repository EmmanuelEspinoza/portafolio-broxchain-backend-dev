using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

[Table("user")]
[Index("correo", Name = "UQ__user__2A586E0BD88A8B2F", IsUnique = true)]
public partial class user
{
    [Key]
    public int id { get; set; }

    [Unicode(false)]
    public string name { get; set; } = null!;

    [Unicode(false)]
    public string rfc { get; set; } = null!;

    [Unicode(false)]
    public string curp { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string correo { get; set; } = null!;

    [Unicode(false)]
    public string telefono { get; set; } = null!;

    [InverseProperty("user")]
    public virtual ICollection<accesoUser> accesoUsers { get; set; } = new List<accesoUser>();

    [InverseProperty("inversionista")]
    public virtual ICollection<flujo> flujos { get; set; } = new List<flujo>();

    [InverseProperty("user")]
    public virtual ICollection<token> tokens { get; set; } = new List<token>();

    [InverseProperty("user")]
    public virtual ICollection<transactionLog> transactionLogs { get; set; } = new List<transactionLog>();
}
