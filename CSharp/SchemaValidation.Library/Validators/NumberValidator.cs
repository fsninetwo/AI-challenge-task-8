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

        /// <summary>
        /// Sets the minimum allowed value (inclusive).
        /// </summary>
        /// <param name="value">The minimum value that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public NumberValidator Min(double value)
        {
            _min = value;
            return this;
        }

        /// <summary>
        /// Sets the maximum allowed value (inclusive).
        /// </summary>
        /// <param name="value">The maximum value that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public NumberValidator Max(double value)
        {
            _max = value;
            return this;
        }

        /// <summary>
        /// Sets the minimum and maximum allowed values (inclusive).
        /// </summary>
        /// <param name="min">The minimum value that will be considered valid</param>
        /// <param name="max">The maximum value that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public NumberValidator SetRange(double min, double max)
        {
            _min = min;
            _max = max;
            return this;
        }

        /// <summary>
        /// Requires the value to be an integer (no decimal places).
        /// Uses epsilon comparison to handle floating-point precision issues.
        /// </summary>
        /// <returns>The validator instance for method chaining</returns>
        public NumberValidator Integer()
        {
            _integer = true;
            return this;
        }

        /// <summary>
        /// Requires the value to be non-negative (greater than or equal to zero).
        /// </summary>
        /// <returns>The validator instance for method chaining</returns>
        public NumberValidator NonNegative()
        {
            _min = 0;
            _nonNegative = true;
            return this;
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
                return CreateError($"Value must be greater than or equal to {_min.Value}");
            }

            if (_max.HasValue && value > _max.Value)
            {
                return CreateError($"Value must be less than or equal to {_max.Value}");
            }

            if (_integer && Math.Abs(value % 1) > double.Epsilon)
            {
                return CreateError("must be an integer");
            }

            if (_nonNegative && value < 0)
            {
                return CreateError("must be non-negative");
            }

            return ValidationResult.Success<double>();
        }
    }
} 