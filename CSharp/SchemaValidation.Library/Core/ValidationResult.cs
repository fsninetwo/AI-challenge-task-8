using System.Collections.Generic;

namespace SchemaValidation.Core;

public sealed class ValidationResult<T>
{
    private ValidationResult(bool isValid, IReadOnlyList<ValidationError>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ValidationError>();
    }

    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }

    public static ValidationResult<T> Success() => new(true);

    public static ValidationResult<T> Failure(string message, string? propertyName = null)
    {
        var error = new ValidationError(message, propertyName);
        return new ValidationResult<T>(false, new[] { error });
    }

    public static ValidationResult<T> Failure(IReadOnlyList<ValidationError> errors)
    {
        return new ValidationResult<T>(false, errors);
    }
}

public sealed record ValidationError(string Message, string? PropertyName = null);

public static class ValidationResult
{
    public static ValidationResult<T> Success<T>() => ValidationResult<T>.Success();

    public static ValidationResult<T> Failure<T>(string message, string? propertyName = null)
        => ValidationResult<T>.Failure(message, propertyName);

    public static ValidationResult<T> Failure<T>(IReadOnlyList<ValidationError> errors)
        => ValidationResult<T>.Failure(errors);
} 