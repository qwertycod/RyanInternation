using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

[Keyless]
[Table("StudGrade")]
public partial class StudGrade
{
    [Column(TypeName = "numeric(18, 0)")]
    public decimal Id { get; set; }

    [MaxLength(50)]
    public byte[]? Subject { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Grade { get; set; }
}
