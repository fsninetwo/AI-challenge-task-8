using System;
using System.Text.RegularExpressions;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
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

    public static class ValidatorExtensions
    {
        public static Validator<object> Pattern(this Validator<object> validator, string pattern)
        {
            if (validator is Schema.ObjectWrapper<string> wrapper && 
                wrapper.GetValidator() is StringValidator stringValidator)
            {
                stringValidator.Pattern(pattern);
                return validator;
            }
            throw new InvalidOperationException("Cannot call Pattern on non-string validator");
        }

        public static Validator<object> MinLength(this Validator<object> validator, int length)
        {
            if (validator is Schema.ObjectWrapper<string> wrapper && 
                wrapper.GetValidator() is StringValidator stringValidator)
            {
                stringValidator.MinLength(length);
                return validator;
            }
            throw new InvalidOperationException("Cannot call MinLength on non-string validator");
        }

        public static Validator<object> MaxLength(this Validator<object> validator, int length)
        {
            if (validator is Schema.ObjectWrapper<string> wrapper && 
                wrapper.GetValidator() is StringValidator stringValidator)
            {
                stringValidator.MaxLength(length);
                return validator;
            }
            throw new InvalidOperationException("Cannot call MaxLength on non-string validator");
        }
    }
} 