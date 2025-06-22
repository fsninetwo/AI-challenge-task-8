using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

/// <summary>
/// Validator for complex objects that validates object properties according to a schema.
/// Supports validation of required/optional properties, nested objects, and property dependencies.
/// </summary>
/// <typeparam name="T">The type of object being validated</typeparam>
/// <remarks>
/// The ObjectValidator provides comprehensive validation for complex objects:
/// - Property validation using schema-defined rules
/// - Required/optional property handling
/// - Nested object validation
/// - Property dependency rules
/// - Additional property validation
/// 
/// Example usage:
/// <code>
/// var schema = new Dictionary&lt;string, Validator&lt;object&gt;&gt;
/// {
///     { "name", Schema.String().MinLength(2) },
///     { "age", Schema.Number().Range(0, 120) }
/// };
/// var validator = new ObjectValidator&lt;User&gt;(schema);
/// validator.MarkPropertyAsOptional("age");
/// </code>
/// </remarks>
public sealed class ObjectValidator<T> : Validator<T> where T : class
{
    private readonly Dictionary<string, (Validator<object> Validator, bool IsRequired)> _schema;
    private readonly HashSet<string> _allowedProperties;
    private bool _allowAdditionalProperties = true;
    private Dictionary<(string, string, string), (Func<T, object?, object?, bool> Rule, string Message)>? _dependencyRules;

    /// <summary>
    /// Initializes a new instance of the ObjectValidator class with a validation schema.
    /// </summary>
    /// <param name="schema">Dictionary defining validation rules for object properties</param>
    /// <exception cref="ArgumentNullException">Thrown when schema is null</exception>
    /// <remarks>
    /// The schema dictionary maps property names to their validators.
    /// By default, all properties in the schema are required unless marked optional.
    /// </remarks>
    public ObjectValidator(Dictionary<string, Validator<object>> schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        _schema = schema.ToDictionary(
            x => x.Key,
            x => (x.Value, true)
        );
        _allowedProperties = new HashSet<string>(schema.Keys);
    }

    /// <summary>
    /// Marks a property as optional in the validation schema.
    /// </summary>
    /// <param name="propertyName">The name of the property to mark as optional</param>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    /// <remarks>
    /// Optional properties are not required to have a value during validation.
    /// If a value is provided, it will still be validated according to the property's validation rules.
    /// </remarks>
    public void MarkPropertyAsOptional(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        if (_schema.ContainsKey(propertyName))
        {
            var (validator, _) = _schema[propertyName];
            _schema[propertyName] = (validator, false);
        }
    }

    /// <summary>
    /// Adds a dependency rule between properties.
    /// </summary>
    /// <param name="propertyName">The name of the property that depends on others</param>
    /// <param name="property1Name">The name of the first property to check</param>
    /// <param name="property2Name">The name of the second property to check</param>
    /// <param name="rule">The validation rule to apply</param>
    /// <param name="message">Optional custom error message</param>
    /// <exception cref="ArgumentException">Thrown when property names are null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when rule is null</exception>
    /// <remarks>
    /// Dependency rules enable complex validation scenarios where the validity of one property
    /// depends on the values of other properties. For example, requiring a phone number to
    /// start with "+1-" when the country is "USA".
    /// </remarks>
    public void AddDependencyRule(string propertyName, string property1Name, string property2Name, Func<T, object?, object?, bool> rule, string? message = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentException.ThrowIfNullOrEmpty(property1Name);
        ArgumentException.ThrowIfNullOrEmpty(property2Name);
        ArgumentNullException.ThrowIfNull(rule);

        _dependencyRules ??= new Dictionary<(string, string, string), (Func<T, object?, object?, bool>, string)>();
        _dependencyRules[(propertyName, property1Name, property2Name)] = (rule, message ?? $"Property '{propertyName}' depends on missing or invalid value of '{property1Name}' or '{property2Name}'");
    }

    /// <summary>
    /// Validates an object according to the schema and dependency rules.
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <returns>A ValidationResult containing validation outcome and any errors</returns>
    /// <remarks>
    /// The validation process:
    /// 1. Checks if the object is null
    /// 2. Validates all required properties
    /// 3. Validates optional properties if they have values
    /// 4. Applies any dependency rules
    /// 5. Checks for disallowed additional properties
    /// </remarks>
    public override ValidationResult<T> Validate(T value)
    {
        if (value == null)
        {
            return CreateError("Object cannot be null");
        }

        var errors = new List<ValidationError>();
        var properties = value.GetType().GetProperties();

        // Validate schema properties
        foreach (var (propertyName, (validator, isRequired)) in _schema)
        {
            var property = properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null)
            {
                if (isRequired)
                {
                    errors.Add(new ValidationError($"Required property '{propertyName}' not found"));
                }
                continue;
            }

            var propertyValue = property.GetValue(value);
            if (propertyValue == null)
            {
                if (isRequired)
                {
                    errors.Add(new ValidationError($"Required property '{propertyName}' cannot be null"));
                }
                continue;
            }

            var result = validator.Validate(propertyValue);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => new ValidationError(e.Message, propertyName)));
            }
        }

        // Check for additional properties
        if (!_allowAdditionalProperties)
        {
            var additionalProperties = properties
                .Where(p => !_allowedProperties.Contains(p.Name))
                .Select(p => p.Name)
                .ToList();

            if (additionalProperties.Any())
            {
                errors.Add(new ValidationError($"Unknown properties found: {string.Join(", ", additionalProperties)}"));
            }
        }

        // Apply dependency rules
        if (_dependencyRules != null)
        {
            foreach (var ((propertyName, property1Name, property2Name), (rule, message)) in _dependencyRules)
            {
                var property1Value = GetPropertyValue(value, property1Name);
                var property2Value = GetPropertyValue(value, property2Name);

                if (!rule(value, property1Value, property2Value))
                {
                    errors.Add(new ValidationError(message));
                }
            }
        }

        return errors.Any()
            ? new ValidationResult<T>(errors)
            : new ValidationResult<T>();
    }

    /// <summary>
    /// Gets a property value using a path expression.
    /// </summary>
    /// <param name="obj">The object to get the property from</param>
    /// <param name="propertyPath">The property path (e.g. "Address.Country")</param>
    /// <returns>The property value, or null if not found</returns>
    /// <remarks>
    /// Supports nested property access using dot notation.
    /// Returns null if any part of the path is null or not found.
    /// </remarks>
    private static object? GetPropertyValue(object obj, string propertyPath)
    {
        var value = obj;
        var parts = propertyPath.Split('.');

        foreach (var part in parts)
        {
            var property = value?.GetType().GetProperty(part);
            if (property == null)
            {
                return null;
            }

            value = property.GetValue(value);
            if (value == null)
            {
                return null;
            }
        }

        return value;
    }

    public override Validator<T> WithMessage(string message)
    {
        base.WithMessage(message);
        foreach (var (validator, _) in _schema.Values)
        {
            validator.WithMessage(message);
        }
        return this;
    }

    private ValidationResult<T> CreateError(string defaultMessage)
    {
        return ValidationResult.Failure<T>(ErrorMessage ?? defaultMessage);
    }

    /// <summary>
    /// Disallows any properties not defined in the validation schema (strict validation).
    /// </summary>
    /// <returns>The current ObjectValidator instance for fluent chaining.</returns>
    /// <remarks>
    /// When strict schema mode is enabled, the validator will add an error if the target
    /// object contains any property that is not explicitly defined in the schema passed
    /// to the constructor. This mirrors the behaviour expected by tests calling the
    /// <c>StrictSchema()</c> helper.
    /// </remarks>
    public ObjectValidator<T> StrictSchema()
    {
        _allowAdditionalProperties = false;
        return this;
    }
}