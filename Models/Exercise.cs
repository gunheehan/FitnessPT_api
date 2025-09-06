using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public int? PrimaryCategoryId { get; set; }

    public int? DifficultyLevel { get; set; }

    public string? TargetMuscles { get; set; }

    public string? Instructions { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercisecategory? PrimaryCategory { get; set; }

    public virtual ICollection<Workoutrecord> Workoutrecords { get; set; } = new List<Workoutrecord>();
}
