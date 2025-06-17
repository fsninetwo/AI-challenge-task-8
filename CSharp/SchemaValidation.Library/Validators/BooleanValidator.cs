using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

/// <summary>
/// Validator for boolean values.
/// Currently only performs type validation, as boolean values are inherently valid once type checking passes.
/// Can be extended in the future to support additional validation rules if needed.
/// </summary>
public sealed class BooleanValidator : Validator<bool>
{
    public override ValidationResult<bool> Validate(bool value)
    {
        return ValidationResult.Success<bool>();
    }
} 