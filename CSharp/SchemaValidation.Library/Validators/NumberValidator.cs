using System;
using SchemaValidation.Core;
using System.Collections.Generic;

namespace SchemaValidation.Library.Validators
{
    /// <summary>
    /// Validator for numeric values that checks range constraints and numeric type requirements.
    /// Supports validation of minimum/maximum values, integer constraints, and non-negative values.
    /// </summary>
    public sealed class NumberValidator : Validator<double>
    {
        private double? _min;
        private double? _max;
        private bool _integer;
        private bool _nonNegative;
        private string? _customErrorMessage;

        /// <summary>
        /// Sets the minimum allowed value (inclusive).
        /// </summary>
        /// <param name="value">The minimum value that will be considered valid</param>
        public void SetMin(double value)
        {
            _min = value;
        }

        /// <summary>
        /// Sets the maximum allowed value (inclusive).
        /// </summary>
        /// <param name="value">The maximum value that will be considered valid</param>
        public void SetMax(double value)
        {
            _max = value;
        }

        /// <summary>
        /// Sets the minimum and maximum allowed values (inclusive).
        /// </summary>
        /// <param name="min">The minimum value that will be considered valid</param>
        /// <param name="max">The maximum value that will be considered valid</param>
        public void SetRange(double min, double max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Requires the value to be an integer (no decimal places).
        /// Uses epsilon comparison to handle floating-point precision issues.
        /// </summary>
        public void SetInteger()
        {
            _integer = true;
        }

        /// <summary>
        /// Requires the value to be non-negative (greater than or equal to zero).
        /// </summary>
        public void SetNonNegative()
        {
            _min = 0;
            _nonNegative = true;
        }

        /// <summary>
        /// Validates a numeric value against all configured rules.
        /// </summary>
        /// <param name="value">The numeric value to validate</param>
        /// <returns>A ValidationResult indicating success or failure with error messages</returns>
        public override ValidationResult<double> Validate(double value)
        {
            if (_min.HasValue && value < _min.Value)
            {
                return CreateError(_customErrorMessage ?? $"Value must be greater than or equal to {_min.Value}");
            }

            if (_max.HasValue && value > _max.Value)
            {
                return CreateError(_customErrorMessage ?? $"Value must be less than or equal to {_max.Value}");
            }

            if (_integer && Math.Abs(value % 1) > double.Epsilon)
            {
                return CreateError(_customErrorMessage ?? "Value must be an integer");
            }

            if (_nonNegative && value < 0)
            {
                return CreateError(_customErrorMessage ?? "Value must be non-negative");
            }

            return ValidationResult.Success<double>();
        }

        public override Validator<double> WithMessage(string message)
        {
            _customErrorMessage = message;
            return this;
        }
    }
} 