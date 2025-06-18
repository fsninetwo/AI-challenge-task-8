using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

public static class ValidatorExtensions
{
    public static Validator<T> Optional<T>(this Validator<T> validator, string propertyName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        if (validator is ObjectValidator<T> objectValidator)
        {
            objectValidator.MarkPropertyAsOptional(propertyName);
        }
        return validator;
    }

    public static DependencyBuilder<T> When<T>(this Validator<T> validator, string propertyName, Func<T, bool> condition) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(condition);
        if (validator is ObjectValidator<T> objectValidator)
        {
            return new DependencyBuilder<T>(objectValidator, propertyName, condition);
        }
        throw new InvalidOperationException("Conditional validation is only supported for object validators.");
    }

    public static StringValidator MinLength(this StringValidator validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator.MinLength(length);
    }

    public static StringValidator MaxLength(this StringValidator validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator.MaxLength(length);
    }

    public static StringValidator Pattern(this StringValidator validator, string pattern)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator.Pattern(pattern);
    }

    public static NumberValidator NonNegative(this NumberValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator.NonNegative();
    }

    public static NumberValidator Min(this NumberValidator validator, double value)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.Min(value);
        return validator;
    }

    public static NumberValidator Max(this NumberValidator validator, double value)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.Max(value);
        return validator;
    }

    public static ArrayValidator<T> MinLength<T>(this ArrayValidator<T> validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.SetMinLength(length);
        return validator;
    }

    public static ArrayValidator<T> MaxLength<T>(this ArrayValidator<T> validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.SetMaxLength(length);
        return validator;
    }

    public static ArrayValidator<T> Unique<T>(this ArrayValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.SetUnique();
        return validator;
    }

    public static ArrayValidator<T> UniqueBy<T>(this ArrayValidator<T> validator, Func<T, T, bool> uniqueBy)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(uniqueBy);
        validator.SetUniqueBy(uniqueBy);
        return validator;
    }

    public static TValidator WithMessage<TValidator>(this TValidator validator, string message)
        where TValidator : Validator<object>
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.WithMessage(message);
        return validator;
    }

    public static Validator<object> MinLength(this Validator<object> validator, int length)
    {
        if (validator is ValidatorWrapper<string, object, StringValidator> stringValidator)
        {
            stringValidator.UnderlyingValidator.MinLength(length);
            return validator;
        }
        if (validator is ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>> arrayValidator)
        {
            arrayValidator.UnderlyingValidator.SetMinLength(length);
            return validator;
        }
        throw new InvalidOperationException("MinLength is only supported for string and array validators.");
    }

    public static Validator<object> MaxLength(this Validator<object> validator, int length)
    {
        if (validator is ValidatorWrapper<string, object, StringValidator> stringValidator)
        {
            stringValidator.UnderlyingValidator.MaxLength(length);
            return validator;
        }
        if (validator is ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>> arrayValidator)
        {
            arrayValidator.UnderlyingValidator.SetMaxLength(length);
            return validator;
        }
        throw new InvalidOperationException("MaxLength is only supported for string and array validators.");
    }

    public static Validator<object> Pattern(this Validator<object> validator, string pattern)
    {
        if (validator is ValidatorWrapper<string, object, StringValidator> stringValidator)
        {
            stringValidator.UnderlyingValidator.Pattern(pattern);
            return validator;
        }
        throw new InvalidOperationException("Pattern is only supported for string validators.");
    }

    /// <summary>
    /// Configures a number validator wrapped as <see cref="Validator{object}"/> to require non-negative values.
    /// </summary>
    /// <param name="validator">The number validator wrapper to extend.</param>
    /// <returns>The same validator instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the underlying validator is not a number validator.</exception>
    public static Validator<object> NonNegative(this Validator<object> validator)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> numValidator)
        {
            numValidator.UnderlyingValidator.NonNegative();
            return validator;
        }
        throw new InvalidOperationException("NonNegative is only supported for number validators.");
    }
} 