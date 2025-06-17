using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Core
{
    public sealed class ValidatorWrapper<TValue, TObject, TValidator> : Validator<TObject>
        where TValidator : Validator<TValue>
    {
        private readonly TValidator _validator;

        public ValidatorWrapper(TValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public TValidator UnderlyingValidator => _validator;

        public override ValidationResult<TObject> Validate(TObject value)
        {
            if (value == null)
            {
                return CreateError(ErrorMessage ?? GetDefaultErrorMessage(typeof(TValue)));
            }

            TValue typedValue;
            try
            {
                if (value is TValue v)
                {
                    typedValue = v;
                }
                else if (typeof(TValue) == typeof(string))
                {
                    if (value is string)
                    {
                        typedValue = (TValue)(object)value;
                    }
                    else
                    {
                        return CreateError(ErrorMessage ?? "Value must be a string");
                    }
                }
                else if (typeof(TValue) == typeof(double))
                {
                    if (value is IConvertible)
                    {
                        try
                        {
                            typedValue = (TValue)(object)Convert.ToDouble(value);
                        }
                        catch
                        {
                            return CreateError(ErrorMessage ?? "Value must be a number");
                        }
                    }
                    else
                    {
                        return CreateError(ErrorMessage ?? "Value must be a number");
                    }
                }
                else if (typeof(TValue) == typeof(bool))
                {
                    if (value is bool b)
                    {
                        typedValue = (TValue)(object)b;
                    }
                    else
                    {
                        return CreateError(ErrorMessage ?? "Value must be a boolean");
                    }
                }
                else if (value is IConvertible)
                {
                    try
                    {
                        typedValue = (TValue)Convert.ChangeType(value, typeof(TValue));
                    }
                    catch
                    {
                        return CreateError(ErrorMessage ?? GetDefaultErrorMessage(typeof(TValue)));
                    }
                }
                else
                {
                    return CreateError(ErrorMessage ?? GetDefaultErrorMessage(typeof(TValue)));
                }
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is NullReferenceException || ex is OverflowException)
            {
                return CreateError(ErrorMessage ?? GetDefaultErrorMessage(typeof(TValue)));
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
            base.WithMessage(message);
            if (_validator != null)
            {
                _validator.WithMessage(message);
            }
            return this;
        }

        private ValidationResult<TObject> CreateError(string message)
        {
            return ValidationResult.Failure<TObject>(new[] { new ValidationError(message) });
        }

        private string GetDefaultErrorMessage(Type type)
        {
            if (type == typeof(double) || type.Name.ToLowerInvariant() == "double")
            {
                return "Value must be a number";
            }
            return $"Value must be a {type.Name.ToLowerInvariant()}";
        }
    }
} 