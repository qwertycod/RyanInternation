using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

public partial class StudentBook
{
    public int StudentId { get; set; }

    public int BookId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PurchaseDate { get; set; }

    [Key]
    public int UpdateId { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("StudentBooks")]
    public virtual Book Book { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("StudentBooks")]
    public virtual Student Student { get; set; } = null!;
}
