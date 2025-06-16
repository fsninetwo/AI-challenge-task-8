using System;
using SchemaValidation.Core;

namespace SchemaValidation.Validators;

public static class ValidatorExtensions
{
    public static Validator<object> Pattern(this Validator<object> validator, string pattern)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        if (validator is Schema.ObjectWrapper<string> wrapper && 
            wrapper.GetValidator() is StringValidator stringValidator)
        {
            stringValidator.Pattern(pattern);
            return validator;
        }
        throw new InvalidOperationException("Cannot call Pattern on non-string validator");
    }

    public static Validator<object> MinLength(this Validator<object> validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (validator is Schema.ObjectWrapper<string> wrapper && 
            wrapper.GetValidator() is StringValidator stringValidator)
        {
            stringValidator.MinLength(length);
            return validator;
        }
        throw new InvalidOperationException("Cannot call MinLength on non-string validator");
    }

    public static Validator<object> MaxLength(this Validator<object> validator, int length)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (validator is Schema.ObjectWrapper<string> wrapper && 
            wrapper.GetValidator() is StringValidator stringValidator)
        {
            stringValidator.MaxLength(length);
            return validator;
        }
        throw new InvalidOperationException("Cannot call MaxLength on non-string validator");
    }
} 