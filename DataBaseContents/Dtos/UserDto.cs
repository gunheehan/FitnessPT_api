namespace FitnessPT_api.DataBaseContents.Dtos;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? GoogleId { get; set; }
    public short Role { get; set; }
    public string RoleName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserProfileDto? Profile { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? GoogleId { get; set; }
    public short Role { get; set; } = 1;
}

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public short? Role { get; set; }
    public bool? IsActive { get; set; }
}