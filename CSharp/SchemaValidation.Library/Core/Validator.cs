namespace SchemaValidation.Core;

public abstract class Validator<T>
{
    protected string? ErrorMessage { get; private set; }

    public abstract ValidationResult<T> Validate(T value);

    public virtual Validator<T> WithMessage(string message)
    {
        ErrorMessage = message;
        return this;
    }

    protected ValidationResult<T> CreateError(string defaultMessage, string? propertyName = null)
    {
        return ValidationResult.Failure<T>(ErrorMessage ?? defaultMessage, propertyName);
    }
} 