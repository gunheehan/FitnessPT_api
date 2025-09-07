using System.Text.Json;

namespace FitnessPT_api.DataBaseContents.Dtos;

public class WorkoutRecordDto
{
    public int RecordId { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public int? ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public DateOnly WorkoutDate { get; set; }
    public string? SetsData { get; set; }
    public int? TotalDurationMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateWorkoutRecordDto
{
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public DateOnly WorkoutDate { get; set; }
    public string? SetsData { get; set; }
    public int? TotalDurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWorkoutRecordDto
{
    public DateOnly? WorkoutDate { get; set; }
    public string SetsData { get; set; }
    public int? TotalDurationMinutes { get; set; }
    public string? Notes { get; set; }
}