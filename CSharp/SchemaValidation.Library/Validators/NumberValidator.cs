using System;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public sealed class NumberValidator : Validator<double>
    {
        private double? _min;
        private double? _max;
        private bool _isInteger;
        private bool _allowNegative = true;

        public NumberValidator Min(double value)
        {
            _min = value;
            return this;
        }

        public NumberValidator Max(double value)
        {
            _max = value;
            return this;
        }

        public NumberValidator Integer()
        {
            _isInteger = true;
            return this;
        }

        public NumberValidator NonNegative()
        {
            _allowNegative = false;
            return this;
        }

        public override ValidationResult<double> Validate(double value)
        {
            if (_isInteger && Math.Abs(value % 1) > double.Epsilon)
            {
                return CreateError("Value must be an integer");
            }

            if (!_allowNegative && value < 0)
            {
                return CreateError("Value cannot be negative");
            }

            if (_min.HasValue && value < _min.Value)
            {
                return CreateError($"Value must be greater than or equal to {_min.Value}");
            }

            if (_max.HasValue && value > _max.Value)
            {
                return CreateError($"Value must be less than or equal to {_max.Value}");
            }

            return ValidationResult.Success<double>();
        }
    }
} 