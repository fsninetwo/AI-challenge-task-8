using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Core
{
    /// <summary>
    /// Wraps a strongly-typed validator to provide type conversion and validation capabilities.
    /// Enables validation of values that may need type conversion before validation.
    /// </summary>
    /// <typeparam name="TValue">The type that the underlying validator validates</typeparam>
    /// <typeparam name="TObject">The type of object being validated (usually object)</typeparam>
    /// <typeparam name="TValidator">The type of the underlying validator</typeparam>
    public sealed class ValidatorWrapper<TValue, TObject, TValidator> : Validator<TObject>
        where TValidator : Validator<TValue>
    {
        private readonly TValidator _validator;

        /// <summary>
        /// Initializes a new instance of the ValidatorWrapper class.
        /// </summary>
        /// <param name="validator">The underlying validator to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
        public ValidatorWrapper(TValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Gets the underlying validator instance.
        /// Useful for accessing specific validation capabilities of the wrapped validator.
        /// </summary>
        public TValidator UnderlyingValidator => _validator;

        /// <summary>
        /// Validates a value, performing type conversion if necessary.
        /// Handles special cases for string and numeric conversions.
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <returns>A ValidationResult containing the validation outcome</returns>
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
                    if (value is bool || value is string)
                    {
                        return CreateError(ErrorMessage ?? "Value must be a number");
                    }
                    try
                    {
                        typedValue = (TValue)(object)Convert.ToDouble(value);
                    }
                    catch (Exception)
                    {
                        return CreateError(ErrorMessage ?? "Value must be a valid number");
                    }
                }
                else if (typeof(TValue) == typeof(bool))
                {
                    if (value is string || !(value is bool))
                    {
                        return CreateError(ErrorMessage ?? "Value must be a boolean");
                    }
                    typedValue = (TValue)value;
                }
                else if (typeof(TValue) == typeof(DateTime))
                {
                    if (value is string dateStr)
                    {
                        if (DateTime.TryParse(dateStr, out var date))
                        {
                            typedValue = (TValue)(object)date;
                        }
                        else
                        {
                            return CreateError(ErrorMessage ?? "Value must be a valid date");
                        }
                    }
                    else if (value is DateTime)
                    {
                        typedValue = (TValue)value;
                    }
                    else
                    {
                        return CreateError(ErrorMessage ?? "Value must be a date");
                    }
                }
                else
                {
                    return CreateError(ErrorMessage ?? $"Cannot convert value to {typeof(TValue).Name}");
                }
            }
            catch (Exception)
            {
                return CreateError(ErrorMessage ?? $"Invalid {typeof(TValue).Name} value");
            }

            var result = _validator.Validate(typedValue);
            if (!result.IsValid)
            {
                return new ValidationResult<TObject>(result.Errors);
            }

            return new ValidationResult<TObject>();
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