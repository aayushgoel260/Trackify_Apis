using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("Holiday")]
public partial class Holiday
{
    [Key]
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int LocationId { get; set; }

    public bool IsOptional { get; set; }

    [ForeignKey("LocationId")]
    //[InverseProperty("Holidays")]
    public virtual Location Location { get; set; } = null!;
}
