using System;
using System.Collections.Generic;
using System.Linq;
using SchemaValidation.Validators;

namespace SchemaValidation.Core
{
    public class Schema
    {
        public class ObjectWrapper<T> : Validator<object>
        {
            private readonly Validator<T> _validator;

            public ObjectWrapper(Validator<T> validator)
            {
                _validator = validator;
            }

            public Validator<T> GetValidator() => _validator;

            public override ValidationResult Validate(object value)
            {
                if (value == null)
                    return new ValidationResult(false, "Value cannot be null");

                if (value is T typedValue)
                    return _validator.Validate(typedValue);

                return new ValidationResult(false, $"Expected type {typeof(T).Name}, got {value.GetType().Name}");
            }
        }

        private class ArrayObjectWrapper : Validator<object>
        {
            private readonly Validator<object> _itemValidator;

            public ArrayObjectWrapper(Validator<object> itemValidator)
            {
                _itemValidator = itemValidator;
            }

            public override ValidationResult Validate(object value)
            {
                if (value == null)
                    return new ValidationResult(false, "Array cannot be null");

                if (value is IEnumerable<object> enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        var result = _itemValidator.Validate(item);
                        if (!result.IsValid)
                            return result;
                    }
                    return new ValidationResult(true);
                }

                return new ValidationResult(false, "Value is not an array");
            }
        }

        public static Validator<object> String() => new ObjectWrapper<string>(new StringValidator());
        public static Validator<object> Number() => new ObjectWrapper<double>(new NumberValidator());
        public static Validator<object> Boolean() => new ObjectWrapper<bool>(new BooleanValidator());
        public static Validator<object> Date() => new ObjectWrapper<DateTime>(new DateValidator());
        public static ObjectValidator<T> Object<T>(Dictionary<string, Validator<object>> schema) => new ObjectValidator<T>(schema);
        public static Validator<object> Array(Validator<object> itemValidator) => new ArrayObjectWrapper(itemValidator);
    }
} 