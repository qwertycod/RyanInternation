using System;
using System.Collections.Generic;

namespace Homework.Models;

public partial class StudentAccount
{
    public int AccountId { get; set; }

    public int StudentId { get; set; }

    public decimal TotalFee { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
