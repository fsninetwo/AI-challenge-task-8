using System.Collections.Generic;

namespace SchemaValidation.Core;

/// <summary>
/// Represents the result of a validation operation for a specific type.
/// Contains information about validation success/failure and any validation errors.
/// </summary>
/// <typeparam name="T">The type of value that was validated</typeparam>
/// <remarks>
/// ValidationResult is a generic class that encapsulates the outcome of a validation operation.
/// It provides:
/// - A boolean flag indicating validation success/failure
/// - A collection of validation errors if validation failed
/// - Factory methods for creating success/failure results
/// - Immutable design for thread safety
/// 
/// Usage example:
/// <code>
/// var result = ValidationResult.Success&lt;string&gt;();
/// var failureResult = ValidationResult.Failure&lt;int&gt;("Invalid number");
/// </code>
/// </remarks>
public sealed class ValidationResult<T>
{
    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    /// <param name="isValid">Whether the validation was successful</param>
    /// <param name="errors">Collection of validation errors (if any)</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods
    /// Success() and Failure() for creating ValidationResult instances.
    /// </remarks>
    public ValidationResult(bool isValid, IReadOnlyList<ValidationError>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ValidationError>();
    }

    /// <summary>
    /// Initializes a successful ValidationResult with no errors.
    /// </summary>
    public ValidationResult() : this(true, null) { }

    /// <summary>
    /// Initializes a failed ValidationResult using the provided error collection.
    /// </summary>
    public ValidationResult(IReadOnlyList<ValidationError> errors) : this(errors == null || errors.Count == 0, errors) { }

    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    /// <remarks>
    /// A true value indicates all validation rules passed.
    /// A false value indicates one or more validation rules failed.
    /// </remarks>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the list of validation errors if validation failed.
    /// </summary>
    /// <remarks>
    /// The list is empty if validation was successful.
    /// Each error contains a message and optionally a property name.
    /// The list is read-only to maintain immutability.
    /// </remarks>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A ValidationResult indicating success</returns>
    /// <remarks>
    /// Use this method when all validation rules pass.
    /// The resulting ValidationResult will have:
    /// - IsValid = true
    /// - Empty Errors collection
    /// </remarks>
    public static ValidationResult<T> Success() => new(true);

    /// <summary>
    /// Creates a failed validation result with a single error message.
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="propertyName">Optional name of the property that failed validation</param>
    /// <returns>A ValidationResult indicating failure with the error message</returns>
    /// <remarks>
    /// Use this method when a validation rule fails.
    /// The resulting ValidationResult will have:
    /// - IsValid = false
    /// - Single error in Errors collection
    /// - Optional property name for identifying which field failed
    /// </remarks>
    public static ValidationResult<T> Failure(string message, string? propertyName = null)
    {
        var error = new ValidationError(message, propertyName);
        return new ValidationResult<T>(false, new[] { error });
    }

    /// <summary>
    /// Creates a failed validation result with multiple error messages.
    /// </summary>
    /// <param name="errors">Collection of validation errors</param>
    /// <returns>A ValidationResult indicating failure with multiple errors</returns>
    /// <remarks>
    /// Use this method when multiple validation rules fail.
    /// The resulting ValidationResult will have:
    /// - IsValid = false
    /// - Multiple errors in Errors collection
    /// This is useful for complex validations where multiple aspects can fail simultaneously.
    /// </remarks>
    public static ValidationResult<T> Failure(IReadOnlyList<ValidationError> errors)
    {
        return new ValidationResult<T>(false, errors);
    }
}

public sealed record ValidationError(string Message, string? PropertyName = null);

public static class ValidationResult
{
    public static ValidationResult<T> Success<T>() => ValidationResult<T>.Success();

    public static ValidationResult<T> Failure<T>(string message, string? propertyName = null)
        => ValidationResult<T>.Failure(message, propertyName);

    public static ValidationResult<T> Failure<T>(IReadOnlyList<ValidationError> errors)
        => ValidationResult<T>.Failure(errors);
}

// TODO: temporary comment 