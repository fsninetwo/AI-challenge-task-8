using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

/// <summary>
/// Validator for arrays of objects that provides object-specific array validation rules.
/// Supports validation of array length, object schema validation, and uniqueness constraints.
/// </summary>
/// <typeparam name="T">The type of objects in the array</typeparam>
public sealed class ObjectArrayValidator<T> : Validator<IEnumerable<T>> where T : class
{
    private readonly ObjectValidator<T> _itemValidator;
    private int? _minLength;
    private int? _maxLength;
    private bool _uniqueItems;
    private Func<T, object?>? _uniqueByProperty;
    private string? _uniquePropertyName;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the ObjectArrayValidator class.
    /// </summary>
    /// <param name="schema">Dictionary defining the validation rules for object properties</param>
    /// <exception cref="ArgumentNullException">Thrown when schema is null</exception>
    public ObjectArrayValidator(Dictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        _itemValidator = Schema.Object<T>(schema);
    }

    /// <summary>
    /// Sets the minimum length requirement for the array.
    /// </summary>
    /// <param name="length">The minimum number of items required</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative</exception>
    public ObjectArrayValidator<T> MinLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _minLength = length;
        return this;
    }

    /// <summary>
    /// Sets the maximum length allowed for the array.
    /// </summary>
    /// <param name="length">The maximum number of items allowed</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative</exception>
    public ObjectArrayValidator<T> MaxLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _maxLength = length;
        return this;
    }

    /// <summary>
    /// Configures the validator to require unique objects in the array.
    /// Objects are compared using their default equality comparison.
    /// </summary>
    /// <returns>The validator instance for method chaining</returns>
    public ObjectArrayValidator<T> Unique()
    {
        _uniqueItems = true;
        return this;
    }

    /// <summary>
    /// Configures the validator to require uniqueness based on a specific property.
    /// </summary>
    /// <param name="propertyName">Name of the property to check for uniqueness</param>
    /// <param name="propertySelector">Function to select the property to check</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when propertySelector is null</exception>
    public ObjectArrayValidator<T> UniqueBy(string propertyName, Func<T, object?> propertySelector)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(propertySelector);
        _uniquePropertyName = propertyName;
        _uniqueByProperty = propertySelector;
        return this;
    }

    /// <summary>
    /// Sets a custom error message for validation failures.
    /// </summary>
    /// <param name="message">The custom error message</param>
    /// <returns>The validator instance for method chaining</returns>
    public override Validator<IEnumerable<T>> WithMessage(string message)
    {
        _errorMessage = message;
        _itemValidator.WithMessage(message);
        return this;
    }

    /// <summary>
    /// Validates an array of objects against all configured rules.
    /// </summary>
    /// <param name="value">The array to validate</param>
    /// <returns>A ValidationResult indicating success or failure with error messages</returns>
    public override ValidationResult<IEnumerable<T>> Validate(IEnumerable<T> value)
    {
        if (value == null)
        {
            return CreateError(_errorMessage ?? "Value must be an array");
        }

        var items = value.ToList();
        var errors = new List<ValidationError>();

        if (_minLength.HasValue && items.Count < _minLength.Value)
        {
            errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Array must have at least {_minLength.Value} items"));
        }

        if (_maxLength.HasValue && items.Count > _maxLength.Value)
        {
            errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Array must have at most {_maxLength.Value} items"));
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

        if (_uniqueByProperty != null)
        {
            var duplicatesByProperty = items
                .Where(x => x != null)
                .GroupBy(_uniqueByProperty)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesByProperty.Any())
            {
                errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Duplicate value found for property {_uniquePropertyName}", _uniquePropertyName));
            }
        }

        for (var idx = 0; idx < items.Count; idx++)
        {
            var item = items[idx];
            if (item == null)
            {
                errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Item at index {idx} cannot be null"));
                continue;
            }

            var itemResult = _itemValidator.Validate(item);
            if (!itemResult.IsValid)
            {
                errors.AddRange(itemResult.Errors.Select(e => new ValidationError(e.Message, e.PropertyName)));
            }
        }

        return errors.Any()
            ? new ValidationResult<IEnumerable<T>>(errors)
            : new ValidationResult<IEnumerable<T>>();
    }

    private ValidationResult<IEnumerable<T>> CreateError(string message)
    {
        return ValidationResult.Failure<IEnumerable<T>>(new List<ValidationError> { new ValidationError(message) });
    }
} 