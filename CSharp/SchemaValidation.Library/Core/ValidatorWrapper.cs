using SchemaValidation.Core;

namespace SchemaValidation.Core
{
    public sealed class ValidatorWrapper<TValue, TObject, TValidator> : Validator<TObject>
        where TValidator : Validator<TValue>
    {
        private readonly TValidator _validator;
        private string? _customErrorMessage;

        public ValidatorWrapper(TValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public TValidator UnderlyingValidator => _validator;

        public override ValidationResult<TObject> Validate(TObject value)
        {
            if (value == null)
            {
                return CreateError($"Expected value of type {typeof(TValue).Name}, got null");
            }

            TValue typedValue;
            try
            {
                if (value is TValue v)
                {
                    typedValue = v;
                }
                else
                {
                    typedValue = (TValue)Convert.ChangeType(value, typeof(TValue));
                }
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
            {
                return CreateError($"Expected value of type {typeof(TValue).Name}, got {value.GetType().Name}");
            }

            var result = _validator.Validate(typedValue);
            if (!result.IsValid)
            {
                return ValidationResult.Failure<TObject>(result.Errors);
            }

            return ValidationResult.Success<TObject>();
        }

        public override Validator<TObject> WithMessage(string message)
        {
            _customErrorMessage = message;
            return this;
        }

        private ValidationResult<TObject> CreateError(string message)
        {
            return ValidationResult.Failure<TObject>(_customErrorMessage ?? message);
        }
    }
} 