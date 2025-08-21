using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

[Keyless]
[Table("RefreshToken")]
public partial class RefreshToken
{
    [StringLength(100)]
    public string Id { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Token { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    public DateOnly? Expires { get; set; }

    public bool? IsRevoked { get; set; }

    public bool? IsUsed { get; set; }
}
