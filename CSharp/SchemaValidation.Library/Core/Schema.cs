using System;
using System.Collections.Generic;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Core;

public static class Schema
{
    public static Validator<object> String()
    {
        return new ValidatorWrapper<string, object, StringValidator>(new StringValidator());
    }

    public static Validator<object> Number()
    {
        return new ValidatorWrapper<double, object, NumberValidator>(new NumberValidator());
    }

    public static Validator<object> Boolean()
    {
        return new ValidatorWrapper<bool, object, BooleanValidator>(new BooleanValidator());
    }

    public static Validator<object> Date()
    {
        return new ValidatorWrapper<DateTime, object, DateValidator>(new DateValidator());
    }

    public static ValidatorWrapper<IEnumerable<T>, object, ArrayValidator<T>> Array<T>(Validator<object> itemValidator)
    {
        ArgumentNullException.ThrowIfNull(itemValidator);
        return new ValidatorWrapper<IEnumerable<T>, object, ArrayValidator<T>>(new ArrayValidator<T>(itemValidator));
    }

    public static Validator<object> ObjectArray<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ValidatorWrapper<IEnumerable<T>, object, ObjectArrayValidator<T>>(new ObjectArrayValidator<T>(schema));
    }

    public static ObjectValidator<T> Object<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ObjectValidator<T>(schema);
    }

    public static Validator<object> ObjectAsValidator<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ValidatorWrapper<T, object, ObjectValidator<T>>(new ObjectValidator<T>(schema));
    }
} 