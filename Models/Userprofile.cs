using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Userprofile
{
    public int UserId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Gender { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? CurrentWeightKg { get; set; }

    public string? FitnessGoal { get; set; }

    public string? FitnessLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
