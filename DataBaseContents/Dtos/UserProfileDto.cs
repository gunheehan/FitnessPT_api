namespace FitnessPT_api.DataBaseContents.Dtos;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public string? FitnessLevel { get; set; }
    public string? FitnessGoal { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserProfileDto
{
    public int UserId { get; set; }
    public string? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public string? FitnessLevel { get; set; }
    public string? FitnessGoal { get; set; }
}

public class UpdateUserProfileDto
{
    public string? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public string? FitnessLevel { get; set; }
    public string? FitnessGoal { get; set; }
}