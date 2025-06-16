namespace SchemaValidation.Core;

public abstract class Validator<T>
{
    protected string? ErrorMessage { get; private set; }

    public Validator<T> WithMessage(string message)
    {
        ErrorMessage = message;
        return this;
    }

    public abstract ValidationResult Validate(T value);
} 