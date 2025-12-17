using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("UserLeave")]
public partial class UserLeave
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public int LeaveTypeId { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [ForeignKey("ActionTypeId")]
    [InverseProperty("UserLeaves")]
    public virtual ActionType ActionType { get; set; } = null!;

    [ForeignKey("Id")]
    [InverseProperty("UserLeave")]
    public virtual User IdNavigation { get; set; } = null!;

    [ForeignKey("LeaveTypeId")]
    [InverseProperty("UserLeaves")]
    public virtual LeaveType LeaveType { get; set; } = null!;
}