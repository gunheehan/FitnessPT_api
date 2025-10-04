using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class RoutineExercise
{
    public int Id { get; set; }

    public int RoutineId { get; set; }

    public int ExerciseId { get; set; }

    /// <summary>
    /// 루틴 내 운동 순서
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// 세트 수
    /// </summary>
    public int? Sets { get; set; }

    /// <summary>
    /// 반복 횟수
    /// </summary>
    public int? Reps { get; set; }

    /// <summary>
    /// 시간 기반 운동(초)
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// 세트 간 휴식(초)
    /// </summary>
    public int? RestSeconds { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual Routine Routine { get; set; } = null!;
}
