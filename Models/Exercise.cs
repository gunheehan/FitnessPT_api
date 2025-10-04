using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Exercise
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Level { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? CategoryDetail { get; set; }

    public string? ImageUrl { get; set; }

    public string? VideoUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
