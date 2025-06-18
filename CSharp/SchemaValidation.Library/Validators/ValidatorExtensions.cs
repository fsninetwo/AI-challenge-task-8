using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

/// <summary>
/// Provides extension methods for validators to enable fluent validation configuration.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Marks a property as optional in object validation.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    /// <param name="validator">The validator to extend</param>
    /// <param name="propertyName">The name of the property to mark as optional</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    public static Validator<T> Optional<T>(this Validator<T> validator, string propertyName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        if (validator is ObjectValidator<T> objectValidator)
        {
            objectValidator.MarkPropertyAsOptional(propertyName);
        }
        return validator;
    }

    /// <summary>
    /// Creates a conditional validation rule for a property.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    /// <param name="validator">The validator to extend</param>
    /// <param name="propertyName">The name of the property to apply the condition to</param>
    /// <param name="condition">The condition that determines when validation should occur</param>
    /// <returns>A DependencyBuilder for configuring the conditional validation</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when condition is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when validator is not an ObjectValidator</exception>
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

    /// <summary>
    /// Sets the minimum length requirement for a string or array.
    /// </summary>
    /// <param name="validator">The validator to extend</param>
    /// <param name="length">The minimum length required</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when validator is not a string or array validator</exception>
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

    /// <summary>
    /// Sets the maximum length allowed for a string or array.
    /// </summary>
    /// <param name="validator">The validator to extend</param>
    /// <param name="length">The maximum length allowed</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when validator is not a string or array validator</exception>
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

    /// <summary>
    /// Sets a pattern requirement for string validation.
    /// </summary>
    /// <param name="validator">The validator to extend</param>
    /// <param name="pattern">The regex pattern to match against</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when validator is not a string validator</exception>
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
    /// Configures a number validator to require non-negative values.
    /// </summary>
    /// <param name="validator">The validator to extend</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null</exception>
    public static NumberValidator NonNegative(this NumberValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator.NonNegative();
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
} 