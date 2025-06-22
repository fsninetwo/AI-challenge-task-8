using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators
{
    /// <summary>
    /// Non-generic array/collection validator that operates on <see cref="IEnumerable{Object}"/>.
    /// This complements the generic <c>ArrayValidator&lt;T&gt;</c> and provides a simpler API
    /// (no type parameter) which is expected by several unit-tests.
    /// </summary>
    /// <remarks>
    /// The implementation delegates most of the heavy-lifting to an internally held
    /// <see cref="Validator{TValue}"/> for the individual items – configured via <see cref="SetItems"/> –
    /// and mirrors the length-checking logic of the generic implementation.
    /// </remarks>
    public sealed class ArrayValidator : Validator<IEnumerable<object>>
    {
        private Validator<object>? _itemValidator;
        private int? _minLength;
        private int? _maxLength;
        private string? _errorMessage;

        /// <summary>
        /// Sets the validator that should be applied to each element in the collection.
        /// </summary>
        /// <param name="itemValidator">Validator for single items.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="itemValidator"/> is null.</exception>
        public void SetItems(Validator<object> itemValidator)
        {
            _itemValidator = itemValidator ?? throw new ArgumentNullException(nameof(itemValidator));
        }

        /// <summary>
        /// Requires the collection to contain at least <paramref name="length"/> elements.
        /// </summary>
        /// <param name="length">Minimum number of items.</param>
        public void SetMinLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _minLength = length;
        }

        /// <summary>
        /// Requires the collection to contain at most <paramref name="length"/> elements.
        /// </summary>
        /// <param name="length">Maximum number of items.</param>
        public void SetMaxLength(int length)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            _maxLength = length;
        }

        /// <inheritdoc />
        public override Validator<IEnumerable<object>> WithMessage(string message)
        {
            _errorMessage = message;
            _itemValidator?.WithMessage(message);
            return this;
        }

        /// <inheritdoc />
        public override ValidationResult<IEnumerable<object>> Validate(IEnumerable<object> value)
        {
            if (value == null)
            {
                return CreateError(_errorMessage ?? "Value must be an array");
            }

            var items = value.ToList();
            var errors = new List<ValidationError>();

            // Length checks
            if (_minLength.HasValue && items.Count < _minLength.Value)
            {
                errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Array must have at least {_minLength.Value} items"));
            }

            if (_maxLength.HasValue && items.Count > _maxLength.Value)
            {
                errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Array must have at most {_maxLength.Value} items"));
            }

            // Item processing
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    errors.Add(new ValidationError(ErrorMessage ?? _errorMessage ?? $"Item at index {i} cannot be null"));
                    continue;
                }

                if (_itemValidator != null)
                {
                    var result = _itemValidator.Validate(item);
                    if (!result.IsValid)
                    {
                        // Prefix errors with item index for clarity
                        errors.AddRange(result.Errors.Select(e => new ValidationError($"Item at index {i}: {e.Message}")));
                    }
                }
            }

            return errors.Any()
                ? new ValidationResult<IEnumerable<object>>(errors)
                : new ValidationResult<IEnumerable<object>>();
        }

        private ValidationResult<IEnumerable<object>> CreateError(string message)
        {
            return ValidationResult.Failure<IEnumerable<object>>(new List<ValidationError>
            {
                new ValidationError(message)
            });
        }
    }
} 