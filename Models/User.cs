using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class User
{
    public int UserId { get; set; }

    public string GoogleId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Routine> Routines { get; set; } = new List<Routine>();
}
