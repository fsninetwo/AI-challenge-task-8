using System;
using System.Collections.Generic;
using System.Reflection;
using SchemaValidation.Core;

namespace SchemaValidation.Validators;

public sealed class ObjectValidator<T> : Validator<T>
{
    private readonly Dictionary<string, Validator<object>> _schema;

    public ObjectValidator(Dictionary<string, Validator<object>> schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    public override ValidationResult Validate(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (_schema.TryGetValue(property.Name, out var validator))
            {
                var propertyValue = property.GetValue(value);
                if (propertyValue is null)
                {
                    return ValidationResult.Failure($"Property '{property.Name}' cannot be null");
                }

                var result = validator.Validate(propertyValue);
                if (!result.IsValid)
                {
                    return ValidationResult.Failure($"Property '{property.Name}': {result.ErrorMessage}");
                }
            }
        }

        return ValidationResult.Success();
    }
} 