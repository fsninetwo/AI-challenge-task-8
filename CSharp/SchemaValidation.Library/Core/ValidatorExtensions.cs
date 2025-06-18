using System;
using System.Collections.Generic;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Core;

/// <summary>
/// Provides extension methods for validators to enable advanced validation scenarios.
/// Includes support for optional properties and dependency-based validation rules.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Marks a property as optional in object validation.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    /// <param name="validator">The validator to extend</param>
    /// <param name="propertyName">The name of the property to mark as optional</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    /// <remarks>
    /// Optional properties are not required to have a value during validation.
    /// If a value is provided, it will still be validated according to the property's validation rules.
    /// </remarks>
    public static ObjectValidator<T> Optional<T>(this ObjectValidator<T> validator, string propertyName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        validator.MarkPropertyAsOptional(propertyName);
        return validator;
    }

    /// <summary>
    /// Creates a dependency rule for conditional validation.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    /// <param name="validator">The validator to extend</param>
    /// <param name="propertyName">The name of the property that depends on other properties</param>
    /// <param name="condition">The condition that determines when the dependency rule applies</param>
    /// <returns>A DependencyBuilder for configuring the dependency rule</returns>
    /// <exception cref="ArgumentException">Thrown when propertyName is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when condition is null</exception>
    /// <remarks>
    /// Use this method to create validation rules that depend on the values of other properties.
    /// The condition determines when the dependency rule should be evaluated.
    /// </remarks>
    public static DependencyBuilder<T> When<T>(this ObjectValidator<T> validator, string propertyName, Func<T, bool> condition) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(condition);
        return new DependencyBuilder<T>(validator, propertyName, condition);
    }
}

/// <summary>
/// Builds dependency rules for object validation.
/// Enables creation of complex validation rules based on property dependencies.
/// </summary>
/// <typeparam name="T">The type of object being validated</typeparam>
/// <remarks>
/// The DependencyBuilder class provides a fluent interface for defining validation rules
/// that depend on the values of other properties. This enables scenarios such as:
/// - Required fields based on other field values
/// - Cross-field validation rules
/// - Complex business logic validation
/// </remarks>
public sealed class DependencyBuilder<T> where T : class
{
    private readonly ObjectValidator<T> _validator;
    private readonly string _propertyName;
    private readonly Func<T, bool> _condition;

    /// <summary>
    /// Initializes a new instance of the DependencyBuilder class.
    /// </summary>
    /// <param name="validator">The validator to add dependency rules to</param>
    /// <param name="propertyName">The name of the property that depends on other properties</param>
    /// <param name="condition">The condition that determines when the dependency rule applies</param>
    /// <remarks>
    /// This constructor is internal and should only be created through the When extension method.
    /// </remarks>
    public DependencyBuilder(ObjectValidator<T> validator, string propertyName, Func<T, bool> condition)
    {
        _validator = validator;
        _propertyName = propertyName;
        _condition = condition;
    }

    /// <summary>
    /// Defines a dependency rule between two properties.
    /// </summary>
    /// <typeparam name="TProperty1">The type of the first property</typeparam>
    /// <typeparam name="TProperty2">The type of the second property</typeparam>
    /// <param name="property1Name">The name of the first property</param>
    /// <param name="property2Name">The name of the second property</param>
    /// <param name="rule">The validation rule to apply when the condition is met</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <exception cref="ArgumentException">Thrown when property names are null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when rule is null</exception>
    /// <remarks>
    /// The dependency rule is evaluated when:
    /// 1. The condition specified in When() is true
    /// 2. Both properties have values
    /// 3. The rule function returns false
    /// 
    /// Example:
    /// <code>
    /// validator.When(nameof(User.HasPhone), u => u.HasPhone)
    ///     .DependsOn&lt;string, string&gt;(
    ///         nameof(User.PhoneNumber),
    ///         nameof(User.CountryCode),
    ///         (phone, country) => !string.IsNullOrEmpty(phone) && phone.StartsWith(country)
    ///     );
    /// </code>
    /// </remarks>
    public ObjectValidator<T> DependsOn<TProperty1, TProperty2>(string property1Name, string property2Name, Func<TProperty1, TProperty2, bool> rule)
    {
        ArgumentException.ThrowIfNullOrEmpty(property1Name);
        ArgumentException.ThrowIfNullOrEmpty(property2Name);
        ArgumentNullException.ThrowIfNull(rule);

        _validator.AddDependencyRule(_propertyName, property1Name, property2Name, 
            (obj, prop1, prop2) => !_condition(obj) || rule((TProperty1)prop1!, (TProperty2)prop2!));

        return _validator;
    }
} 