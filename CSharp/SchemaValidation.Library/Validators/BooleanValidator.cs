using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

/// <summary>
/// Validator for boolean values.
/// Currently only performs type validation, as boolean values are inherently valid once type checking passes.
/// Can be extended in the future to support additional validation rules if needed.
/// </summary>
public sealed class BooleanValidator : Validator<bool>
{
    private string? _customErrorMessage;

    public override ValidationResult<bool> Validate(bool value)
    {
        // Boolean values are inherently valid once type checking passes
        return ValidationResult.Success<bool>();
    }

    public override Validator<bool> WithMessage(string message)
    {
        _customErrorMessage = message;
        return this;
    }

    public string GetErrorMessage(string defaultMessage)
    {
        return _customErrorMessage ?? defaultMessage;
    }
} 