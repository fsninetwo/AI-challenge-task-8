using SchemaValidation.Core;

namespace SchemaValidation.Validators;

public sealed class BooleanValidator : Validator<bool>
{
    public override ValidationResult<bool> Validate(bool value)
    {
        return ValidationResult.Success<bool>();
    }
} 