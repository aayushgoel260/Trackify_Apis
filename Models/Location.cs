using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrackifyApis.Models;

[Table("Location")]
public partial class Location
{
    [Key]
    public int Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    //[InverseProperty("Location")]
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();

    //[InverseProperty("Location")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
