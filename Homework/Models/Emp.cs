using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

[Keyless]
[Table("Emp")]
public partial class Emp
{
    [Column(TypeName = "numeric(18, 0)")]
    public decimal Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? EmpName { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? ManagerId { get; set; }

    public DateOnly? DateOfJoining { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? City { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? DpName { get; set; }
}
