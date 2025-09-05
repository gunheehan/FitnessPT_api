namespace FitnessPT_api.GoogleAuth.Models;

public class GoogleAuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public GoogleUser User { get; set; } = new();
    public bool IsNewUser { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}