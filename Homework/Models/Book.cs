using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class Book
{
    [Key]
    public int BookId { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    public int? AuthorId { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    public int? PublishedYear { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Books")]
    public virtual Author? Author { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<StudentBook> StudentBooks { get; set; } = new List<StudentBook>();
}
