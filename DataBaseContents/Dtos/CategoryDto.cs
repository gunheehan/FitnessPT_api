
namespace FitnessPT_api.DataBaseContents.Dtos;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryCode { get; set; } = null!;
    public int? DisplayOrder { get; set; }
    public List<CategoryDto>? SubCategories { get; set; }
    public List<ExerciseDto>? Exercises { get; set; }
}

public class CreateCategoryDto
{
    public int? ParentCategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryCode { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

public class UpdateCategoryDto
{
    public int? ParentCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryCode { get; set; }
    public int? DisplayOrder { get; set; }
}