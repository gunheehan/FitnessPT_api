using System.ComponentModel.DataAnnotations;

namespace FitnessPT_api.GoogleAuth.Models;

public class GoogleAuthRequest
{
    [Required(ErrorMessage = "Google 토큰이 필요합니다.")]
    public string GoogleToken { get; set; } = string.Empty;
}