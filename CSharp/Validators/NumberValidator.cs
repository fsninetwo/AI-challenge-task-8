using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public class NumberValidator : Validator<double>
    {
        public override ValidationResult Validate(double value)
        {
            return new ValidationResult(true);
        }
    }
} 