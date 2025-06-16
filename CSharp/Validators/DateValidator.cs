using System;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public class DateValidator : Validator<DateTime>
    {
        public override ValidationResult Validate(DateTime value)
        {
            return new ValidationResult(true);
        }
    }
} 