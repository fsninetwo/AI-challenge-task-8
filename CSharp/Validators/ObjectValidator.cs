using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SchemaValidation.Core;

namespace SchemaValidation.Validators;

public sealed class ObjectValidator<T> : Validator<T>
{
    private readonly Dictionary<string, (Validator<object> Validator, bool IsRequired)> _schema;
    private readonly HashSet<string> _allowedProperties;
    private bool _allowAdditionalProperties = true;
    private Dictionary<string, Func<T, bool>>? _conditionalValidations;
    private Dictionary<(string, string), Func<object?, object?, bool>>? _propertyDependencies;

    public ObjectValidator(Dictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        _schema = schema.ToDictionary(
            x => x.Key,
            x => (x.Value, true)
        );
        _allowedProperties = new HashSet<string>(schema.Keys);
    }

    public ObjectValidator<T> Optional(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        if (_schema.ContainsKey(propertyName))
        {
            var (validator, _) = _schema[propertyName];
            _schema[propertyName] = (validator, false);
        }
        return this;
    }

    public ObjectValidator<T> StrictSchema()
    {
        _allowAdditionalProperties = false;
        return this;
    }

    public ObjectValidator<T> When(string propertyName, Func<T, bool> condition)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(condition);

        _conditionalValidations ??= new Dictionary<string, Func<T, bool>>();
        _conditionalValidations[propertyName] = condition;
        return this;
    }

    public ObjectValidator<T> DependsOn(string propertyName, string dependsOnProperty, Func<object?, object?, bool> validator)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentException.ThrowIfNullOrEmpty(dependsOnProperty);
        ArgumentNullException.ThrowIfNull(validator);

        _propertyDependencies ??= new Dictionary<(string, string), Func<object?, object?, bool>>();
        _propertyDependencies[(propertyName, dependsOnProperty)] = validator;
        return this;
    }

    public override ValidationResult Validate(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyDict = properties.ToDictionary(p => p.Name, p => p);

        // Check for unknown properties if strict schema is enabled
        if (!_allowAdditionalProperties)
        {
            var unknownProperties = propertyDict.Keys.Except(_allowedProperties).ToList();
            if (unknownProperties.Any())
            {
                return ValidationResult.Failure(
                    ErrorMessage ?? $"Unknown properties found: {string.Join(", ", unknownProperties)}");
            }
        }

        // Validate each property in the schema
        foreach (var (propertyName, (validator, isRequired)) in _schema)
        {
            if (!propertyDict.TryGetValue(propertyName, out var propertyInfo))
            {
                if (isRequired)
                {
                    return ValidationResult.Failure(
                        ErrorMessage ?? $"Required property '{propertyName}' is missing");
                }
                continue;
            }

            var propertyValue = propertyInfo.GetValue(value);

            // Check if property should be validated based on conditions
            if (_conditionalValidations?.TryGetValue(propertyName, out var condition) == true)
            {
                if (!condition(value))
                {
                    continue; // Skip validation if condition is not met
                }
            }

            // Validate property dependencies
            if (_propertyDependencies is not null)
            {
                var dependencies = _propertyDependencies
                    .Where(x => x.Key.Item1 == propertyName)
                    .ToList();

                foreach (var ((_, dependsOnProperty), dependencyValidator) in dependencies)
                {
                    if (!propertyDict.TryGetValue(dependsOnProperty, out var dependentPropertyInfo))
                    {
                        return ValidationResult.Failure(
                            ErrorMessage ?? $"Property '{propertyName}' depends on missing property '{dependsOnProperty}'");
                    }

                    var dependentValue = dependentPropertyInfo.GetValue(value);
                    if (!dependencyValidator(propertyValue, dependentValue))
                    {
                        return ValidationResult.Failure(
                            ErrorMessage ?? $"Property '{propertyName}' failed dependency validation with '{dependsOnProperty}'");
                    }
                }
            }

            if (propertyValue is null)
            {
                if (isRequired)
                {
                    return ValidationResult.Failure(
                        ErrorMessage ?? $"Required property '{propertyName}' cannot be null");
                }
                continue;
            }

            var result = validator.Validate(propertyValue);
            if (!result.IsValid)
            {
                return ValidationResult.Failure(
                    ErrorMessage ?? $"Property '{propertyName}': {result.ErrorMessage}");
            }
        }

        return ValidationResult.Success();
    }
} 