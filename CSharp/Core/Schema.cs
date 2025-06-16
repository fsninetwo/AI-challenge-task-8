using System;
using System.Collections.Generic;
using SchemaValidation.Validators;

namespace SchemaValidation.Core;

public static class Schema
{
    public sealed class ObjectWrapper<T> : Validator<object>
    {
        private readonly Validator<T> _validator;

        public ObjectWrapper(Validator<T> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public Validator<T> GetValidator() => _validator;

        public override ValidationResult Validate(object value)
        {
            if (value is null)
                return ValidationResult.Failure("Value cannot be null");

            if (value is T typedValue)
                return _validator.Validate(typedValue);

            return ValidationResult.Failure($"Expected type {typeof(T).Name}, got {value.GetType().Name}");
        }
    }

    private sealed class ArrayObjectWrapper : Validator<object>
    {
        private readonly ArrayValidator<object> _validator;

        public ArrayObjectWrapper(ArrayValidator<object> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public override ValidationResult Validate(object value)
        {
            if (value is null)
                return ValidationResult.Failure("Array cannot be null");

            if (value is IEnumerable<object> enumerable)
                return _validator.Validate(enumerable);

            return ValidationResult.Failure("Value is not an array");
        }
    }

    public static Validator<object> String() => new ObjectWrapper<string>(new StringValidator());
    public static Validator<object> Number() => new ObjectWrapper<double>(new NumberValidator());
    public static Validator<object> Boolean() => new ObjectWrapper<bool>(new BooleanValidator());
    public static Validator<object> Date() => new ObjectWrapper<DateTime>(new DateValidator());
    
    public static ObjectValidator<T> Object<T>(IDictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ObjectValidator<T>(new Dictionary<string, Validator<object>>(schema));
    }

    public static Validator<object> ObjectAsValidator<T>(IDictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ObjectWrapper<T>(new ObjectValidator<T>(new Dictionary<string, Validator<object>>(schema)));
    }

    public static Validator<object> Array(Validator<object> itemValidator, Action<ArrayValidator<object>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(itemValidator);
        
        var validator = new ArrayValidator<object>(itemValidator);
        configure?.Invoke(validator);
        return new ArrayObjectWrapper(validator);
    }
} 