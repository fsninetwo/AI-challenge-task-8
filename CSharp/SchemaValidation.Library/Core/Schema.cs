using System;
using System.Collections.Generic;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Core;

/// <summary>
/// Static factory class for creating various types of validators.
/// Provides a fluent interface for building validation schemas.
/// This class serves as the main entry point for creating validation rules.
/// </summary>
/// <remarks>
/// The Schema class follows the factory pattern to create strongly-typed validators
/// for different data types. Each validator is wrapped in a ValidatorWrapper to provide
/// type conversion capabilities while maintaining type safety.
/// </remarks>
public static class Schema
{
    /// <summary>
    /// Creates a string validator for validating string values.
    /// </summary>
    /// <returns>A validator configured for string validation</returns>
    /// <remarks>
    /// The string validator supports:
    /// - Length validation (min/max)
    /// - Pattern matching (regex)
    /// - Email validation
    /// - URL validation
    /// - Custom error messages
    /// </remarks>
    public static Validator<object> String()
    {
        return new ValidatorWrapper<string, object, StringValidator>(new StringValidator());
    }

    /// <summary>
    /// Creates a number validator for validating numeric values.
    /// </summary>
    /// <returns>A validator configured for number validation</returns>
    /// <remarks>
    /// The number validator supports:
    /// - Range validation (min/max)
    /// - Integer validation
    /// - Non-negative validation
    /// - Custom error messages
    /// </remarks>
    public static Validator<object> Number()
    {
        return new ValidatorWrapper<double, object, NumberValidator>(new NumberValidator());
    }

    /// <summary>
    /// Creates a boolean validator for validating boolean values.
    /// </summary>
    /// <returns>A validator configured for boolean validation</returns>
    /// <remarks>
    /// The boolean validator supports:
    /// - Basic boolean validation
    /// - Custom error messages
    /// Future enhancements may include:
    /// - String to boolean conversion
    /// - Custom boolean representations
    /// </remarks>
    public static Validator<object> Boolean()
    {
        return new ValidatorWrapper<bool, object, BooleanValidator>(new BooleanValidator());
    }

    /// <summary>
    /// Creates a date validator for validating DateTime values.
    /// </summary>
    /// <returns>A validator configured for date validation</returns>
    /// <remarks>
    /// The date validator supports:
    /// - Date range validation
    /// - Min/max date constraints
    /// - Custom error messages
    /// </remarks>
    public static Validator<object> Date()
    {
        return new ValidatorWrapper<DateTime, object, DateValidator>(new DateValidator());
    }

    /// <summary>
    /// Creates an array validator for validating collections of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the array</typeparam>
    /// <param name="itemValidator">The validator to use for individual items</param>
    /// <returns>A validator configured for array validation</returns>
    /// <exception cref="ArgumentNullException">Thrown when itemValidator is null</exception>
    /// <remarks>
    /// The array validator supports:
    /// - Length validation (min/max)
    /// - Item validation using provided validator
    /// - Uniqueness validation
    /// - Custom error messages
    /// </remarks>
    public static ValidatorWrapper<IEnumerable<T>, object, ArrayValidator<T>> Array<T>(Validator<object> itemValidator)
    {
        ArgumentNullException.ThrowIfNull(itemValidator);
        return new ValidatorWrapper<IEnumerable<T>, object, ArrayValidator<T>>(new ArrayValidator<T>(itemValidator));
    }

    /// <summary>
    /// Creates a validator for arrays of objects with a specified schema.
    /// </summary>
    /// <typeparam name="T">The type of objects in the array</typeparam>
    /// <param name="schema">Dictionary defining the validation rules for object properties</param>
    /// <returns>A validator configured for object array validation</returns>
    /// <exception cref="ArgumentNullException">Thrown when schema is null</exception>
    /// <remarks>
    /// The object array validator supports:
    /// - Array length validation
    /// - Object schema validation
    /// - Uniqueness validation
    /// - Property-based uniqueness
    /// - Custom error messages
    /// </remarks>
    public static Validator<object> ObjectArray<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ValidatorWrapper<IEnumerable<T>, object, ObjectArrayValidator<T>>(new ObjectArrayValidator<T>(schema));
    }

    /// <summary>
    /// Creates an object validator with a specified schema.
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="schema">Dictionary defining the validation rules for object properties</param>
    /// <returns>A validator configured for object validation</returns>
    /// <exception cref="ArgumentNullException">Thrown when schema is null</exception>
    /// <remarks>
    /// The object validator supports:
    /// - Property validation using schema
    /// - Required/optional properties
    /// - Nested object validation
    /// - Property dependencies
    /// - Custom error messages
    /// </remarks>
    public static ObjectValidator<T> Object<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ObjectValidator<T>(schema);
    }

    /// <summary>
    /// Creates an object validator wrapped as a generic validator.
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="schema">Dictionary defining the validation rules for object properties</param>
    /// <returns>A wrapped object validator</returns>
    /// <exception cref="ArgumentNullException">Thrown when schema is null</exception>
    /// <remarks>
    /// This method is useful when:
    /// - Object validation needs to be composed with other validators
    /// - Type conversion is needed
    /// - Validation needs to be part of a larger schema
    /// </remarks>
    public static Validator<object> ObjectAsValidator<T>(Dictionary<string, Validator<object>> schema) where T : class
    {
        ArgumentNullException.ThrowIfNull(schema);
        return new ValidatorWrapper<T, object, ObjectValidator<T>>(new ObjectValidator<T>(schema));
    }
} 