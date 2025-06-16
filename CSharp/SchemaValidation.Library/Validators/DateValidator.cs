using System;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public sealed class DateValidator : Validator<DateTime>
    {
        private DateTime? _minDate;
        private DateTime? _maxDate;

        public DateValidator MinDate(DateTime date)
        {
            _minDate = date;
            return this;
        }

        public DateValidator MaxDate(DateTime date)
        {
            _maxDate = date;
            return this;
        }

        public override ValidationResult<DateTime> Validate(DateTime value)
        {
            if (_minDate.HasValue && value < _minDate.Value)
            {
                return CreateError($"Date must be after {_minDate.Value:d}");
            }

            if (_maxDate.HasValue && value > _maxDate.Value)
            {
                return CreateError($"Date must be before {_maxDate.Value:d}");
            }

            return ValidationResult.Success<DateTime>();
        }
    }
} 