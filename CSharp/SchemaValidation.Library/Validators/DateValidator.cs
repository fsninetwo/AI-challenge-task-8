using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators
{
    /// <summary>
    /// Validator for DateTime values that checks date range constraints.
    /// Supports validation of minimum and maximum dates.
    /// </summary>
    public sealed class DateValidator : Validator<DateTime>
    {
        private DateTime? _min;
        private DateTime? _max;

        /// <summary>
        /// Sets the minimum allowed date (inclusive).
        /// </summary>
        /// <param name="value">The earliest date that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public DateValidator MinDate(DateTime value)
        {
            _min = value;
            return this;
        }

        /// <summary>
        /// Sets the maximum allowed date (inclusive).
        /// </summary>
        /// <param name="value">The latest date that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public DateValidator MaxDate(DateTime value)
        {
            _max = value;
            return this;
        }

        /// <summary>
        /// Validates a DateTime value against all configured rules.
        /// </summary>
        /// <param name="value">The DateTime value to validate</param>
        /// <returns>A ValidationResult indicating success or failure with error messages</returns>
        public override ValidationResult<DateTime> Validate(DateTime value)
        {
            if (_min.HasValue && value < _min.Value)
            {
                return CreateError($"must be greater than or equal to {_min.Value:d}");
            }

            if (_max.HasValue && value > _max.Value)
            {
                return CreateError($"must be less than or equal to {_max.Value:d}");
            }

            return ValidationResult.Success<DateTime>();
        }
    }
} 