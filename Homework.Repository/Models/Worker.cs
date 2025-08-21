using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Repository.Models;

[Keyless]
public partial class Worker
{
    [StringLength(10)]
    public string WorkerId { get; set; } = null!;

    [StringLength(10)]
    public string? Name { get; set; }

    [StringLength(10)]
    public string? WorkType { get; set; }
}
