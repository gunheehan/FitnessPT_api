using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Workoutrecord
{
    public int RecordId { get; set; }

    public int? UserId { get; set; }

    public DateOnly WorkoutDate { get; set; }

    public int? ExerciseId { get; set; }

    public string? SetsData { get; set; }

    public int? TotalDurationMinutes { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual User? User { get; set; }
}
