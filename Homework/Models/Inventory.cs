using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

[Table("Inventory")]
public partial class Inventory
{
    [Key]
    public int BookId { get; set; }

    [StringLength(255)]
    public string BookName { get; set; } = null!;

    [StringLength(255)]
    public string? Author { get; set; }

    [StringLength(100)]
    public string? Genre { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    public int Count { get; set; }

    [StringLength(255)]
    public string? Publisher { get; set; }

    public DateOnly? PublishedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public int? ChangedByStudentId { get; set; }
}
