using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SchemaValidation
{
    public class Validator<T>
    {
        protected string ErrorMessage { get; set; }

        public Validator<T> WithMessage(string message)
        {
            ErrorMessage = message;
            return this;
        }

        public virtual ValidationResult Validate(T value)
        {
            return new ValidationResult(true);
        }
    }

    public class StringValidator : Validator<string>
    {
        private int? _minLength;
        private int? _maxLength;
        private string _pattern;

        public StringValidator MinLength(int length)
        {
            _minLength = length;
            return this;
        }

        public StringValidator MaxLength(int length)
        {
            _maxLength = length;
            return this;
        }

        public StringValidator Pattern(string pattern)
        {
            _pattern = pattern;
            return this;
        }

        public override ValidationResult Validate(string value)
        {
            if (value == null)
                return new ValidationResult(false, ErrorMessage ?? "Value cannot be null");

            if (_minLength.HasValue && value.Length < _minLength.Value)
                return new ValidationResult(false, ErrorMessage ?? $"Minimum length is {_minLength.Value}");

            if (_maxLength.HasValue && value.Length > _maxLength.Value)
                return new ValidationResult(false, ErrorMessage ?? $"Maximum length is {_maxLength.Value}");

            if (_pattern != null && !Regex.IsMatch(value, _pattern))
                return new ValidationResult(false, ErrorMessage ?? "Pattern validation failed");

            return new ValidationResult(true);
        }
    }

    public class NumberValidator : Validator<double>
    {
        public override ValidationResult Validate(double value)
        {
            return new ValidationResult(true);
        }
    }

    public class BooleanValidator : Validator<bool>
    {
        public override ValidationResult Validate(bool value)
        {
            return new ValidationResult(true);
        }
    }

    public class DateValidator : Validator<DateTime>
    {
        public override ValidationResult Validate(DateTime value)
        {
            return new ValidationResult(true);
        }
    }

    public class ObjectValidator<T> : Validator<T>
    {
        private readonly Dictionary<string, Validator<object>> _schema;

        public ObjectValidator(Dictionary<string, Validator<object>> schema)
        {
            _schema = schema;
        }

        public override ValidationResult Validate(T value)
        {
            // Implementation would validate object properties based on schema
            return new ValidationResult(true);
        }
    }

    public class ArrayValidator<T> : Validator<IEnumerable<T>>
    {
        private readonly Validator<T> _itemValidator;

        public ArrayValidator(Validator<T> itemValidator)
        {
            _itemValidator = itemValidator;
        }

        public override ValidationResult Validate(IEnumerable<T> value)
        {
            if (value == null)
                return new ValidationResult(false, ErrorMessage ?? "Array cannot be null");

            foreach (var item in value)
            {
                var result = _itemValidator.Validate(item);
                if (!result.IsValid)
                    return result;
            }

            return new ValidationResult(true);
        }
    }

    public class Schema
    {
        public static StringValidator String() => new StringValidator();
        public static NumberValidator Number() => new NumberValidator();
        public static BooleanValidator Boolean() => new BooleanValidator();
        public static DateValidator Date() => new DateValidator();
        public static ObjectValidator<T> Object<T>(Dictionary<string, Validator<object>> schema) => new ObjectValidator<T>(schema);
        public static ArrayValidator<T> Array<T>(Validator<T> itemValidator) => new ArrayValidator<T>(itemValidator);
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
} 