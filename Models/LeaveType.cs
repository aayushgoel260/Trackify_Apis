using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("LeaveType")]
public partial class LeaveType
{
    [Key]
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string LeaveCategory { get; set; } = null!;

    [InverseProperty("LeaveType")]
    public virtual ICollection<UserLeave> UserLeaves { get; set; } = new List<UserLeave>();
}
