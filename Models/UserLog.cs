using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Keyless]
[Table("UserLog")]
public partial class UserLog
{
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    public int LocationId { get; set; }

    public bool IsActive { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime InsertDt { get; set; }
}
