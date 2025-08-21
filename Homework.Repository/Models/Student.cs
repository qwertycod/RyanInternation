using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

public partial class Student
{
    [Key]
    public int StudentId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    public int? Age { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Course { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Year { get; set; }

    [Column("GPA", TypeName = "decimal(3, 2)")]
    public decimal? Gpa { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? PaymentStatus { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [InverseProperty("Student")]
    public virtual ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentBook> StudentBooks { get; set; } = new List<StudentBook>();

    [InverseProperty("Student")]
    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();
}
