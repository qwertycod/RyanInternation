using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Homework.Models;

public partial class SalesDatum
{
    [Key]
    [Column("SaleID")]
    public int SaleId { get; set; }

    [Column("ProductID")]
    public int ProductId { get; set; }

    [Column("RegionID")]
    public int RegionId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? SaleAmount { get; set; }
}
