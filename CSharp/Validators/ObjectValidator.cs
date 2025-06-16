using System;
using System.Collections.Generic;
using SchemaValidation.Core;

namespace SchemaValidation.Validators
{
    public class ObjectValidator<T> : Validator<T>
    {
        private readonly Dictionary<string, Validator<object>> _schema;

        public ObjectValidator(Dictionary<string, Validator<object>> schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public override ValidationResult Validate(T value)
        {
            if (value == null)
                return new ValidationResult(false, ErrorMessage ?? "Object cannot be null");

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (_schema.TryGetValue(property.Name, out var validator))
                {
                    var propertyValue = property.GetValue(value);
                    var result = validator.Validate(propertyValue);
                    if (!result.IsValid)
                        return new ValidationResult(false, $"Property '{property.Name}': {result.ErrorMessage}");
                }
            }

            return new ValidationResult(true);
        }
    }
} 