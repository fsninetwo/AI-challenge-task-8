using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators
{
    /// <summary>
    /// Validator for arrays and collections that provides array-specific validation rules.
    /// Supports validation of array length, item validation, and uniqueness constraints.
    /// </summary>
    /// <typeparam name="T">The type of items in the array</typeparam>
    public sealed class ArrayValidator<T> : Validator<IEnumerable<T>>
    {
        private readonly Validator<object> _itemValidator;
        private int? _minLength;
        private int? _maxLength;
        private bool _uniqueItems;
        private Func<T, object?>? _uniqueByProperty;
        private string? _uniquePropertyName;
        private string? _errorMessage;

        /// <summary>
        /// Initializes a new instance of the ArrayValidator class.
        /// </summary>
        /// <param name="itemValidator">The validator to use for individual items in the array</param>
        /// <exception cref="ArgumentNullException">Thrown when itemValidator is null</exception>
        public ArrayValidator(Validator<object> itemValidator)
        {
            _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
        }

        /// <summary>
        /// Sets the minimum length requirement for the array.
        /// </summary>
        /// <param name="length">The minimum number of items required</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative</exception>
        public void SetMinLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _minLength = length;
        }

        /// <summary>
        /// Sets the maximum length allowed for the array.
        /// </summary>
        /// <param name="length">The maximum number of items allowed</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative</exception>
        public void SetMaxLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _maxLength = length;
        }

        /// <summary>
        /// Configures the validator to require unique items in the array.
        /// </summary>
        public void SetUnique()
        {
            _uniqueItems = true;
        }

        /// <summary>
        /// Configures the validator to require uniqueness based on a specific property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property to check for uniqueness</typeparam>
        /// <param name="propertySelector">Function to select the property to check</param>
        /// <param name="propertyName">Name of the property being checked</param>
        /// <exception cref="ArgumentNullException">Thrown when propertySelector is null</exception>
        /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
        public void SetUniqueBy<TProperty>(Func<T, TProperty> propertySelector, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertySelector);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            _uniqueByProperty = x => propertySelector(x);
            _uniquePropertyName = propertyName;
        }

        /// <summary>
        /// Configures the validator to use a custom comparison for uniqueness checking.
        /// </summary>
        /// <param name="comparer">The comparison function to use</param>
        /// <exception cref="ArgumentNullException">Thrown when comparer is null</exception>
        public void SetUniqueBy(Func<T, T, bool> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);
            _uniqueItems = true;
        }

        /// <summary>
        /// Validates an array against all configured rules.
        /// </summary>
        /// <param name="value">The array to validate</param>
        /// <returns>A ValidationResult indicating success or failure with error messages</returns>
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
                    errors.Add(new ValidationError(_errorMessage ?? $"Duplicate value found for property {_uniquePropertyName}"));
                }
            }

            foreach (var item in items)
            {
                if (item != null)
                {
                    var itemResult = _itemValidator.Validate(item);
                    if (!itemResult.IsValid)
                    {
                        errors.AddRange(itemResult.Errors);
                    }
                }
            }

            return errors.Any()
                ? new ValidationResult<IEnumerable<T>>(errors)
                : new ValidationResult<IEnumerable<T>>();
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