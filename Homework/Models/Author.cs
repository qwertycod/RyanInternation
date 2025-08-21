using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class Author
{
    [Key]
    public int AuthorId { get; set; }

    [StringLength(100)]
    public string? AuthorName { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
