using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FondoInversion.Models;

public partial class transactionLog
{
    [Key]
    public int id { get; set; }

    [Unicode(false)]
    public string requestBody { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime fecha { get; set; }

    public int userID { get; set; }

    [ForeignKey("userID")]
    [InverseProperty("transactionLogs")]
    public virtual user user { get; set; } = null!;
}
