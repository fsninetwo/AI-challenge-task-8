using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public sealed class ArrayValidator<T> : Validator<IEnumerable<T>>
    {
        private readonly Validator<T> _itemValidator;
        private int? _minLength;
        private int? _maxLength;
        private bool _uniqueItems;
        private Func<T, T, bool>? _uniqueBy;

        public ArrayValidator(Validator<T> itemValidator)
        {
            _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
        }

        public ArrayValidator<T> MinLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _minLength = length;
            return this;
        }

        public ArrayValidator<T> MaxLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _maxLength = length;
            return this;
        }

        public ArrayValidator<T> Unique()
        {
            _uniqueItems = true;
            return this;
        }

        public ArrayValidator<T> UniqueBy(Func<T, T, bool> comparer)
        {
            _uniqueBy = comparer ?? throw new ArgumentNullException(nameof(comparer));
            return this;
        }

        public override ValidationResult<IEnumerable<T>> Validate(IEnumerable<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var items = value.ToList();
            var errors = new List<ValidationError>();

            if (_minLength.HasValue && items.Count < _minLength.Value)
            {
                errors.Add(new ValidationError($"Array must have at least {_minLength.Value} items"));
                return ValidationResult.Failure<IEnumerable<T>>(errors);
            }

            if (_maxLength.HasValue && items.Count > _maxLength.Value)
            {
                errors.Add(new ValidationError($"Array must have at most {_maxLength.Value} items"));
                return ValidationResult.Failure<IEnumerable<T>>(errors);
            }

            if (_uniqueItems)
            {
                var duplicates = items
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    errors.Add(new ValidationError("Array contains duplicate items"));
                    return ValidationResult.Failure<IEnumerable<T>>(errors);
                }
            }

            if (_uniqueBy != null)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    for (var j = i + 1; j < items.Count; j++)
                    {
                        if (_uniqueBy(items[i], items[j]))
                        {
                            errors.Add(new ValidationError("Array contains items that are considered duplicates by the custom comparer"));
                            return ValidationResult.Failure<IEnumerable<T>>(errors);
                        }
                    }
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                var itemResult = _itemValidator.Validate(items[i]);
                if (!itemResult.IsValid)
                {
                    errors.AddRange(itemResult.Errors.Select(e => new ValidationError(
                        $"Item at index {i}: {e.Message}",
                        $"[{i}]")));
                }
            }

            return errors.Count > 0
                ? ValidationResult.Failure<IEnumerable<T>>(errors)
                : ValidationResult.Success<IEnumerable<T>>();
        }
    }
} 