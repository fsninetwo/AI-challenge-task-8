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
        private DateTime? _minDate;
        private DateTime? _maxDate;

        /// <summary>
        /// Sets the minimum allowed date (inclusive).
        /// </summary>
        /// <param name="minDate">The earliest date that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public DateValidator Min(DateTime minDate)
        {
            _minDate = minDate;
            return this;
        }

        /// <summary>
        /// Sets the maximum allowed date (inclusive).
        /// </summary>
        /// <param name="maxDate">The latest date that will be considered valid</param>
        /// <returns>The validator instance for method chaining</returns>
        public DateValidator Max(DateTime maxDate)
        {
            _maxDate = maxDate;
            return this;
        }

        /// <summary>
        /// Validates a DateTime value against all configured rules.
        /// </summary>
        /// <param name="value">The DateTime value to validate</param>
        /// <returns>A ValidationResult indicating success or failure with error messages</returns>
        public override ValidationResult<DateTime> Validate(DateTime value)
        {
            if (_minDate.HasValue && value < _minDate.Value)
            {
                return CreateError($"Date must be greater than or equal to {_minDate.Value:d}");
            }

            if (_maxDate.HasValue && value > _maxDate.Value)
            {
                return CreateError($"Date must be less than or equal to {_maxDate.Value:d}");
            }

            return ValidationResult.Success<DateTime>();
        }
    }
} 