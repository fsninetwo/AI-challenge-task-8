using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators
{
    public sealed class ArrayValidator : Validator<IEnumerable<object>>
    {
        private int? _minLength;
        private int? _maxLength;
        private Validator<object>? _itemValidator;

        public void SetMinLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _minLength = length;
        }

        public void SetMaxLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _maxLength = length;
        }

        public void SetItems(Validator<object> itemValidator)
        {
            _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
        }

        public override ValidationResult<IEnumerable<object>> Validate(IEnumerable<object> value)
        {
            if (value == null)
                return CreateError("Value must be an array");

            var items = value.ToList();
            var errors = new List<ValidationError>();

            if (_minLength.HasValue && items.Count < _minLength.Value)
            {
                errors.Add(new ValidationError(ErrorMessage ?? $"Array must have at least {_minLength.Value} items"));
            }

            if (_maxLength.HasValue && items.Count > _maxLength.Value)
            {
                errors.Add(new ValidationError(ErrorMessage ?? $"Array must have at most {_maxLength.Value} items"));
            }

            if (_itemValidator != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null)
                    {
                        errors.Add(new ValidationError($"Item at index {i} cannot be null", $"[{i}]"));
                        continue;
                    }

                    var itemResult = _itemValidator.Validate(item);
                    if (!itemResult.IsValid)
                    {
                        errors.AddRange(itemResult.Errors.Select(e => new ValidationError(
                            $"Item at index {i}: {e.Message}",
                            $"[{i}]")));
                    }
                }
            }

            return errors.Count > 0
                ? ValidationResult.Failure<IEnumerable<object>>(errors)
                : ValidationResult.Success<IEnumerable<object>>();
        }
    }

    public sealed class ArrayValidator<T> : Validator<IEnumerable<T>>
    {
        private readonly Validator<object> _itemValidator;
        private int? _minLength;
        private int? _maxLength;
        private bool _uniqueItems;
        private Func<T, object?>? _uniqueByProperty;
        private string? _uniquePropertyName;
        private string? _errorMessage;

        public ArrayValidator(Validator<object> itemValidator)
        {
            _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
        }

        public void SetMinLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _minLength = length;
        }

        public void SetMaxLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _maxLength = length;
        }

        public void SetUnique()
        {
            _uniqueItems = true;
        }

        public void SetUniqueBy<TProperty>(Func<T, TProperty> propertySelector, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertySelector);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            _uniqueByProperty = x => propertySelector(x);
            _uniquePropertyName = propertyName;
        }

        public void SetUniqueBy(Func<T, T, bool> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);
            _uniqueItems = true;
        }

        public override ValidationResult<IEnumerable<T>> Validate(IEnumerable<T> value)
        {
            if (value == null)
            {
                return CreateError("Value must be an array");
            }

            var items = value.ToList();
            var errors = new List<ValidationError>();

            if (_minLength.HasValue && items.Count < _minLength.Value)
            {
                errors.Add(new ValidationError(_errorMessage ?? $"Array must have at least {_minLength.Value} items"));
            }

            if (_maxLength.HasValue && items.Count > _maxLength.Value)
            {
                errors.Add(new ValidationError(_errorMessage ?? $"Array must have at most {_maxLength.Value} items"));
            }

            if (_uniqueItems)
            {
                var duplicates = items
                    .Where(x => x != null)
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    errors.Add(new ValidationError(_errorMessage ?? "Array contains duplicate items"));
                }
            }

            if (_uniqueByProperty != null && _uniquePropertyName != null)
            {
                var duplicatesByProperty = items
                    .Where(x => x != null)
                    .Select((item, index) => new { Item = item, Index = index, Value = _uniqueByProperty(item) })
                    .GroupBy(x => x.Value)
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var group in duplicatesByProperty)
                {
                    var indices = group.Select(x => x.Index).ToList();
                    errors.Add(new ValidationError(
                        _errorMessage ?? $"Duplicate value '{group.Key}' for property '{_uniquePropertyName}' at indices: {string.Join(", ", indices)}",
                        _uniquePropertyName));
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    errors.Add(new ValidationError($"Item at index {i} cannot be null", $"[{i}]"));
                    continue;
                }

                var itemResult = _itemValidator.Validate(item);
                if (!itemResult.IsValid)
                {
                    errors.AddRange(itemResult.Errors.Select(e => new ValidationError(
                        $"Item at index {i}: {e.Message}",
                        $"[{i}]")));
                }
            }

            return errors.Any()
                ? ValidationResult.Failure<IEnumerable<T>>(errors)
                : ValidationResult.Success<IEnumerable<T>>();
        }

        public override Validator<IEnumerable<T>> WithMessage(string message)
        {
            _errorMessage = message;
            return this;
        }

        private ValidationResult<IEnumerable<T>> CreateError(string message)
        {
            return ValidationResult.Failure<IEnumerable<T>>(new List<ValidationError> { new ValidationError(_errorMessage ?? message) });
        }
    }
} 