using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Keyless]
[Table("UserLeaveLog")]
public partial class UserLeaveLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public int LeaveTypeId { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime InsertDt { get; set; }
}
