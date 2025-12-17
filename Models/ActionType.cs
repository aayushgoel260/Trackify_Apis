using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("ActionType")]
public partial class ActionType
{
    [Key]
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string ActionName { get; set; } = null!;

    [InverseProperty("ActionType")]
    public virtual ICollection<UserLeave> UserLeaves { get; set; } = new List<UserLeave>();

    [InverseProperty("ActionType")]
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

    [InverseProperty("ActionType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
