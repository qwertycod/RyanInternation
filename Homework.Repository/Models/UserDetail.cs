using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

public partial class UserDetail
{
    [Key]
    public int UserDetailId { get; set; }

    public int StudentId { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(512)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("UserDetails")]
    public virtual Student Student { get; set; } = null!;
}
