using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("UserProject")]
public partial class UserProject
{
    [Key]
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public int ActionTypeId { get; set; }

    public DateOnly ActionDate { get; set; }

    [ForeignKey("ActionTypeId")]
    [InverseProperty("UserProjects")]
    public virtual ActionType ActionType { get; set; } = null!;

    [ForeignKey("Id")]
    [InverseProperty("UserProject")]
    public virtual User IdNavigation { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("UserProjects")]
    public virtual Project Project { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("UserProjects")]
    public virtual Role Role { get; set; } = null!;
}
