using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class FeePayment
{
    [Key]
    public int PaymentId { get; set; }

    public int? StudentId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PaymentMode { get; set; }

    [StringLength(250)]
    public string? Remarks { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("FeePayments")]
    public virtual Student? Student { get; set; }
}
