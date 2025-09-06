using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class User
{
    public int UserId { get; set; }

    public string GoogleId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public short? Role { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Bodyrecord> Bodyrecords { get; set; } = new List<Bodyrecord>();

    public virtual Userprofile? Userprofile { get; set; }

    public virtual ICollection<Workoutrecord> Workoutrecords { get; set; } = new List<Workoutrecord>();
}
