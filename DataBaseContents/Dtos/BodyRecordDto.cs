namespace FitnessPT_api.DataBaseContents.Dtos;

public class BodyRecordDto
{
    public int RecordId { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public DateOnly RecordedDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? MuscleMassKg { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBodyRecordDto
{
    public int UserId { get; set; }
    public DateOnly RecordedDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? MuscleMassKg { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBodyRecordDto
{
    public DateOnly? RecordedDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? MuscleMassKg { get; set; }
    public string? Notes { get; set; }
}