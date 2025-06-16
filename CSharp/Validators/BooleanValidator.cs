using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public class BooleanValidator : Validator<bool>
    {
        public override ValidationResult Validate(bool value)
        {
            return new ValidationResult(true);
        }
    }
} 