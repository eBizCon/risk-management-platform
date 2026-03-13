using FluentValidation.Results;

namespace RiskManagement.Application.Common;

public static class ValidationHelper
{
    public static Dictionary<string, string[]> ToValidationErrors(ValidationResult validationResult)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var failure in validationResult.Errors)
        {
            var key = ToCamelCase(failure.PropertyName);
            if (!errors.ContainsKey(key))
                errors[key] = Array.Empty<string>();
            errors[key] = errors[key].Append(failure.ErrorMessage).ToArray();
        }

        return errors;
    }

    public static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}