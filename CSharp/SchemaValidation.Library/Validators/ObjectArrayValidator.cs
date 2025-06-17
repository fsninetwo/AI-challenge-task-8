using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

public sealed class ObjectArrayValidator<T> : Validator<IEnumerable<T>> where T : class
{
    private readonly ObjectValidator<T> _itemValidator;
    private int? _minLength;
    private int? _maxLength;
    private bool _uniqueItems;
    private Func<T, object?>? _uniqueByProperty;
    private string? _uniquePropertyName;

    public ObjectArrayValidator(Dictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        _itemValidator = Schema.Object<T>(schema);
    }

    public ObjectArrayValidator<T> MinLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _minLength = length;
        return this;
    }

    public ObjectArrayValidator<T> MaxLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _maxLength = length;
        return this;
    }

    public ObjectArrayValidator<T> Unique()
    {
        _uniqueItems = true;
        return this;
    }

    public ObjectArrayValidator<T> UniqueBy(string propertyName, Func<T, object?> propertySelector)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(propertySelector);
        _uniquePropertyName = propertyName;
        _uniqueByProperty = propertySelector;
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

        if (_uniqueByProperty != null && _uniquePropertyName != null)
        {
            var duplicatesByProperty = items
                .Select((item, index) => new { Item = item, Index = index, Value = _uniqueByProperty(item) })
                .GroupBy(x => x.Value)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in duplicatesByProperty)
            {
                var indices = group.Select(x => x.Index).ToList();
                errors.Add(new ValidationError(
                    $"Duplicate value '{group.Key}' for property '{_uniquePropertyName}' at indices: {string.Join(", ", indices)}",
                    _uniquePropertyName));
            }

            if (errors.Any())
            {
                return ValidationResult.Failure<IEnumerable<T>>(errors);
            }
        }

        for (var i = 0; i < items.Count; i++)
        {
            var itemResult = _itemValidator.Validate(items[i]);
            if (!itemResult.IsValid)
            {
                errors.AddRange(itemResult.Errors.Select(e => new ValidationError(
                    $"Item at index {i}: {e.Message}",
                    $"[{i}].{e.PropertyName}")));
            }
        }

        return errors.Count > 0
            ? ValidationResult.Failure<IEnumerable<T>>(errors)
            : ValidationResult.Success<IEnumerable<T>>();
    }
} 