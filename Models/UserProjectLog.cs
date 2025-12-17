using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Keyless]
[Table("UserProjectLog")]
public partial class UserProjectLog
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public DateOnly UserId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime InsertDt { get; set; }
}
