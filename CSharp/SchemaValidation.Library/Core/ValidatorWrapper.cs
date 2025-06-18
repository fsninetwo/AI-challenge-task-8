using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Core
{
    /// <summary>
    /// Wraps a strongly-typed validator (<typeparamref name="TValidator"/>) and exposes it as a <see cref="Validator{TObject}"/>.
    /// Performs runtime type-conversion where necessary so that callers can feed <c>object</c> values while still re-using the underlying
    /// validator's rich domain-specific rules.
    /// </summary>
    /// <typeparam name="TValue">The value type the underlying validator expects (e.g. <c>string</c>, <c>double</c>).</typeparam>
    /// <typeparam name="TObject">The externally exposed type — usually <c>object</c> so it can plug into heterogeneous schemas.</typeparam>
    /// <typeparam name="TValidator">Concrete validator type (e.g. <c>StringValidator</c>, <c>NumberValidator</c>).</typeparam>
    /// <remarks>
    /// Typical usage is via the <see cref="Schema"/> factory helpers:
    /// <code>
    /// var stringValidator = Schema.String();               // internally a ValidatorWrapper&lt;string, object, StringValidator&gt;
    /// var numberValidator = Schema.Number();               // wraps NumberValidator
    /// </code>
    /// The wrapper takes care of converting incoming <c>object</c> values to <typeparamref name="TValue"/> before delegating
    /// to the underlying <typeparamref name="TValidator"/>.  If conversion fails a descriptive validation error is returned.
    /// </remarks>
    public sealed class ValidatorWrapper<TValue, TObject, TValidator> : Validator<TObject>
        where TValidator : Validator<TValue>
    {
        private readonly TValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorWrapper{TValue, TObject, TValidator}"/> class.
        /// </summary>
        /// <param name="validator">The concrete validator to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <c>null</c>.</exception>
        public ValidatorWrapper(TValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Gets the underlying, strongly-typed validator instance.
        /// Useful when you need to access advanced configuration APIs not exposed by the wrapper.
        /// </summary>
        public TValidator UnderlyingValidator => _validator;

        /// <inheritdoc/>
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
                else if (value is DateTime)
                {
                    typedValue = (TValue)(object)value;
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