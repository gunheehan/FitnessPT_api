using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Routine
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Level { get; set; }

    public string? Category { get; set; }

    /// <summary>
    /// 예상 소요 시간(분)
    /// </summary>
    public int? EstimatedDuration { get; set; }

    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// NULL이면 관리자, 값이 있으면 사용자
    /// </summary>
    public int? CreatedUser { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? CreatedUserNavigation { get; set; }

    public virtual ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
