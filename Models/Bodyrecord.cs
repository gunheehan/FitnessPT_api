using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Bodyrecord
{
    public int RecordId { get; set; }

    public int UserId { get; set; }

    public DateOnly RecordedDate { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? BodyFatPercentage { get; set; }

    public decimal? MuscleMassKg { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
