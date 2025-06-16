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
    private Dictionary<(string, string, string), Func<T, object?, object?, bool>>? _dependencyRules;

    public ObjectValidator(Dictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        _schema = schema.ToDictionary(
            x => x.Key,
            x => (x.Value, true)
        );
        _allowedProperties = new HashSet<string>(schema.Keys);
    }

    public void MarkPropertyAsOptional(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        if (_schema.ContainsKey(propertyName))
        {
            var (validator, _) = _schema[propertyName];
            _schema[propertyName] = (validator, false);
        }
    }

    public void AddDependencyRule(string propertyName, string property1Name, string property2Name, Func<T, object?, object?, bool> rule)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentException.ThrowIfNullOrEmpty(property1Name);
        ArgumentException.ThrowIfNullOrEmpty(property2Name);
        ArgumentNullException.ThrowIfNull(rule);

        _dependencyRules ??= new Dictionary<(string, string, string), Func<T, object?, object?, bool>>();
        _dependencyRules[(propertyName, property1Name, property2Name)] = rule;
    }

    public ObjectValidator<T> StrictSchema()
    {
        _allowAdditionalProperties = false;
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

            // Validate property dependencies
            if (_dependencyRules is not null)
            {
                var dependencies = _dependencyRules
                    .Where(x => x.Key.Item1 == propertyName)
                    .ToList();

                foreach (var ((_, property1Name, property2Name), rule) in dependencies)
                {
                    if (!propertyDict.TryGetValue(property1Name, out var property1Info) ||
                        !propertyDict.TryGetValue(property2Name, out var property2Info))
                    {
                        return ValidationResult.Failure(
                            ErrorMessage ?? $"Property '{propertyName}' depends on missing properties '{property1Name}' or '{property2Name}'");
                    }

                    var property1Value = property1Info.GetValue(value);
                    var property2Value = property2Info.GetValue(value);

                    if (!rule(value, property1Value, property2Value))
                    {
                        return ValidationResult.Failure(
                            ErrorMessage ?? $"Property '{propertyName}' requires a valid phone number when the country is USA");
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