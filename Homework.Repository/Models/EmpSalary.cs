using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

[Keyless]
[Table("EmpSalary")]
public partial class EmpSalary
{
    [Column(TypeName = "numeric(18, 0)")]
    public decimal EmpId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Project { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? Salary { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? Variable { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? Grade { get; set; }
}
