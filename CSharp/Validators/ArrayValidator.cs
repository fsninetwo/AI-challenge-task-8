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
        private bool _allowDuplicates = true;
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

        public ArrayValidator<T> Length(int exactLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(exactLength);
            _minLength = exactLength;
            _maxLength = exactLength;
            return this;
        }

        public ArrayValidator<T> Unique()
        {
            _allowDuplicates = false;
            return this;
        }

        public ArrayValidator<T> UniqueBy(Func<T, T, bool> comparer)
        {
            _uniqueBy = comparer ?? throw new ArgumentNullException(nameof(comparer));
            return this;
        }

        public override ValidationResult Validate(IEnumerable<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var items = value.ToList();

            if (_minLength.HasValue && items.Count < _minLength.Value)
                return ValidationResult.Failure(ErrorMessage ?? $"Array must have at least {_minLength.Value} items");

            if (_maxLength.HasValue && items.Count > _maxLength.Value)
                return ValidationResult.Failure(ErrorMessage ?? $"Array must have at most {_maxLength.Value} items");

            if (!_allowDuplicates)
            {
                var duplicates = items
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                    return ValidationResult.Failure(ErrorMessage ?? "Array contains duplicate items");
            }

            if (_uniqueBy is not null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        if (_uniqueBy(items[i], items[j]))
                            return ValidationResult.Failure(ErrorMessage ?? "Array contains items that are considered duplicates by the custom comparer");
                    }
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                var result = _itemValidator.Validate(items[i]);
                if (!result.IsValid)
                    return ValidationResult.Failure(ErrorMessage ?? $"Item at index {i}: {result.ErrorMessage}");
            }

            return ValidationResult.Success();
        }
    }
} 