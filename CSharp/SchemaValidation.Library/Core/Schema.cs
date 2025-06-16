using System;
using System.Collections.Generic;
using SchemaValidation.Validators;

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

    public static Validator<object> Array<T>(Validator<T> itemValidator)
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

public sealed class ValidatorWrapper<TValue, TObject, TValidator> : Validator<TObject>
    where TValidator : Validator<TValue>
{
    private readonly TValidator _validator;

    public ValidatorWrapper(TValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public TValidator UnderlyingValidator => _validator;

    public override ValidationResult<TObject> Validate(TObject value)
    {
        if (value is not TValue typedValue)
        {
            return CreateError($"Expected value of type {typeof(TValue).Name}");
        }

        var result = _validator.Validate(typedValue);
        return result.IsValid
            ? ValidationResult.Success<TObject>()
            : ValidationResult.Failure<TObject>(result.Errors);
    }
} 