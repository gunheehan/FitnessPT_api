namespace FitnessPT_api.Services;

public interface IValidationService
{
    bool IsValidLevel(string level);
    bool IsVaildCategory(string category);
}

public class ValidationService : IValidationService
{
    public bool IsValidLevel(string level)
    {
        throw new NotImplementedException();
    }

    public bool IsVaildCategory(string category)
    {
        throw new NotImplementedException();
    }
}