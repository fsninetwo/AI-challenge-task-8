using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public class ArrayValidator<T> : Validator<IEnumerable<T>>
    {
        private readonly Validator<T> _itemValidator;

        public ArrayValidator(Validator<T> itemValidator)
        {
            _itemValidator = itemValidator;
        }

        public override ValidationResult Validate(IEnumerable<T> value)
        {
            if (value == null)
                return new ValidationResult(false, ErrorMessage ?? "Array cannot be null");

            foreach (var item in value)
            {
                var result = _itemValidator.Validate(item);
                if (!result.IsValid)
                    return result;
            }

            return new ValidationResult(true);
        }
    }
} 