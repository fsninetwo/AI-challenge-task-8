using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Validators;

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

    public static Validator<object> Pattern(this Validator<object> validator, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        if (validator is ValidatorWrapper<string, object, StringValidator> wrapper)
        {
            wrapper.UnderlyingValidator.Pattern(pattern);
            return validator;
        }
        throw new InvalidOperationException("Pattern can only be used with string validators");
    }

    public static Validator<object> MinLength(this Validator<object> validator, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (validator is ValidatorWrapper<string, object, StringValidator> stringWrapper)
        {
            stringWrapper.UnderlyingValidator.MinLength(length);
            return validator;
        }
        if (validator is ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>> arrayWrapper)
        {
            arrayWrapper.UnderlyingValidator.MinLength(length);
            return validator;
        }
        throw new InvalidOperationException("MinLength can only be used with string or array validators");
    }

    public static Validator<object> MaxLength(this Validator<object> validator, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (validator is ValidatorWrapper<string, object, StringValidator> stringWrapper)
        {
            stringWrapper.UnderlyingValidator.MaxLength(length);
            return validator;
        }
        if (validator is ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>> arrayWrapper)
        {
            arrayWrapper.UnderlyingValidator.MaxLength(length);
            return validator;
        }
        throw new InvalidOperationException("MaxLength can only be used with string or array validators");
    }

    public static Validator<object> Min(this Validator<object> validator, double value)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> wrapper)
        {
            wrapper.UnderlyingValidator.Min(value);
            return validator;
        }
        throw new InvalidOperationException("Min can only be used with number validators");
    }

    public static Validator<object> Max(this Validator<object> validator, double value)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> wrapper)
        {
            wrapper.UnderlyingValidator.Max(value);
            return validator;
        }
        throw new InvalidOperationException("Max can only be used with number validators");
    }

    public static Validator<object> Integer(this Validator<object> validator)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> wrapper)
        {
            wrapper.UnderlyingValidator.Integer();
            return validator;
        }
        throw new InvalidOperationException("Integer can only be used with number validators");
    }

    public static Validator<object> NonNegative(this Validator<object> validator)
    {
        if (validator is ValidatorWrapper<double, object, NumberValidator> wrapper)
        {
            wrapper.UnderlyingValidator.NonNegative();
            return validator;
        }
        throw new InvalidOperationException("NonNegative can only be used with number validators");
    }

    public static Validator<object> WithMessage(this Validator<object> validator, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        if (validator is ValidatorWrapper<string, object, StringValidator> stringWrapper)
        {
            stringWrapper.UnderlyingValidator.WithMessage(message);
            return validator;
        }
        if (validator is ValidatorWrapper<double, object, NumberValidator> numberWrapper)
        {
            numberWrapper.UnderlyingValidator.WithMessage(message);
            return validator;
        }
        if (validator is ValidatorWrapper<bool, object, BooleanValidator> boolWrapper)
        {
            boolWrapper.UnderlyingValidator.WithMessage(message);
            return validator;
        }
        if (validator is ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>> arrayWrapper)
        {
            arrayWrapper.UnderlyingValidator.WithMessage(message);
            return validator;
        }
        throw new InvalidOperationException("WithMessage can only be used with primitive type validators");
    }
} 