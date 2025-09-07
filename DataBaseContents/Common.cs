namespace FitnessPT_api.DataBaseContents;

public enum DifficultyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum UserRole
{
    User = 1,
    Trainer = 2,
    Admin = 3
}

public static class EnumExtensions
{
    public static string GetDifficultyName(this int? difficultyLevel)
    {
        return difficultyLevel switch
        {
            1 => "초급",
            2 => "중급",
            3 => "고급",
            4 => "전문가",
            _ => "미정"
        };
    }

    public static string GetRoleName(this short role)
    {
        return role switch
        {
            1 => "사용자",
            2 => "트레이너",
            3 => "관리자",
            _ => "사용자"
        };
    }
}