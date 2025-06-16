using System;

namespace SchemaValidation.Core
{
    public class Validator<T>
    {
        protected string ErrorMessage { get; set; }

        public Validator<T> WithMessage(string message)
        {
            ErrorMessage = message;
            return this;
        }

        public virtual ValidationResult Validate(T value)
        {
            return new ValidationResult(true);
        }
    }
} 