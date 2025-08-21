using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class StudentGrade
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("subject")]
    [StringLength(100)]
    public string? Subject { get; set; }

    [Column("grade", TypeName = "decimal(5, 2)")]
    public decimal? Grade { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? StudentId { get; set; }
}
