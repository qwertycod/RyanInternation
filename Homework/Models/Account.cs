using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class Account
{
    [Key]
    public int AccountId { get; set; }

    public int StudentId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalFee { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalPaid { get; set; }

    [Column(TypeName = "decimal(11, 2)")]
    public decimal? Balance { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastPaymentDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Accounts")]
    public virtual Student Student { get; set; } = null!;
}
