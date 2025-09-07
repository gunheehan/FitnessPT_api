namespace FitnessPT_api.DataBaseContents.Dtos;

    public class ExerciseDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = null!;
        public int? PrimaryCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryCode { get; set; }
        public int? DifficultyLevel { get; set; }
        public string? DifficultyName { get; set; }
        public string? TargetMuscles { get; set; }
        public string? Instructions { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateExerciseDto
    {
        public string ExerciseName { get; set; } = null!;
        public int? PrimaryCategoryId { get; set; }
        public int? DifficultyLevel { get; set; }
        public string? TargetMuscles { get; set; }
        public string? Instructions { get; set; }
    }

    public class UpdateExerciseDto
    {
        public string? ExerciseName { get; set; }
        public int? PrimaryCategoryId { get; set; }
        public int? DifficultyLevel { get; set; }
        public string? TargetMuscles { get; set; }
        public string? Instructions { get; set; }
        public bool? IsActive { get; set; }
    }
