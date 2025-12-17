using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("User")]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    public int LocationId { get; set; }

    public bool IsActive { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [ForeignKey("ActionTypeId")]
    [InverseProperty("Users")]
    public virtual ActionType ActionType { get; set; } = null!;

    [ForeignKey("LocationId")]
    //[InverseProperty("Users")]
    public virtual Location Location { get; set; } = null!;

    [InverseProperty("IdNavigation")]
    public virtual UserLeave? UserLeave { get; set; }

    [InverseProperty("IdNavigation")]
    public virtual UserProject? UserProject { get; set; }
}