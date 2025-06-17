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
        return validator.MinLength(length);
    }

    public static StringValidator MaxLength(this StringValidator validator, int length)
    {
        return validator.MaxLength(length);
    }

    public static StringValidator Pattern(this StringValidator validator, string pattern)
    {
        return validator.Pattern(pattern);
    }

    public static NumberValidator NonNegative(this NumberValidator validator)
    {
        return validator.NonNegative();
    }

    public static NumberValidator Min(this NumberValidator validator, double value)
    {
        return validator.Min(value);
    }

    public static NumberValidator Max(this NumberValidator validator, double value)
    {
        return validator.Max(value);
    }

    public static ArrayValidator<T> MinLength<T>(this ArrayValidator<T> validator, int length)
    {
        return validator.MinLength(length);
    }

    public static ArrayValidator<T> MaxLength<T>(this ArrayValidator<T> validator, int length)
    {
        return validator.MaxLength(length);
    }

    public static ArrayValidator<T> Unique<T>(this ArrayValidator<T> validator)
    {
        return validator.Unique();
    }

    public static ArrayValidator<T> UniqueBy<T>(this ArrayValidator<T> validator, Func<T, T, bool> uniqueBy)
    {
        return validator.UniqueBy(uniqueBy);
    }

    public static TValidator WithMessage<TValidator>(this TValidator validator, string message)
        where TValidator : Validator<object>
    {
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
            arrayValidator.UnderlyingValidator.MinLength(length);
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
            arrayValidator.UnderlyingValidator.MaxLength(length);
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

    public static Validator<object> NonNegative(this Validator<object> validator)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> numberValidator)
        {
            numberValidator.UnderlyingValidator.NonNegative();
            return validator;
        }
        throw new InvalidOperationException("NonNegative is only supported for number validators.");
    }
} 