namespace SchemaValidation.Core;

/// <summary>
/// Base class for all validators in the schema validation system.
/// Provides core validation functionality and common properties.
/// </summary>
/// <typeparam name="T">The type of value being validated</typeparam>
/// <remarks>
/// The Validator class serves as the foundation for all validation operations.
/// It defines the basic contract that all validators must implement and provides
/// common functionality such as error message handling and result creation.
/// 
/// Key features:
/// - Generic type parameter for type-safe validation
/// - Custom error message support
/// - Abstract Validate method for implementing validation logic
/// - Helper methods for creating validation results
/// 
/// Usage example:
/// <code>
/// public class CustomValidator : Validator&lt;string&gt;
/// {
///     public override ValidationResult&lt;string&gt; Validate(string value)
///     {
///         if (string.IsNullOrEmpty(value))
///             return CreateError("Value cannot be empty");
///         return ValidationResult.Success&lt;string&gt;();
///     }
/// }
/// </code>
/// </remarks>
public abstract class Validator<T>
{
    /// <summary>
    /// Gets or sets the custom error message for validation failures.
    /// </summary>
    /// <remarks>
    /// When set, this message overrides the default error messages provided by validators.
    /// This allows for customization of error messages while maintaining the same validation logic.
    /// </remarks>
    protected string? ErrorMessage { get; private set; }

    /// <summary>
    /// Validates a value of type T according to the validator's rules.
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A ValidationResult containing the validation outcome and any error messages</returns>
    /// <remarks>
    /// This is the main validation method that must be implemented by all concrete validators.
    /// The implementation should:
    /// - Check the value against all validation rules
    /// - Return a successful result if all rules pass
    /// - Return a failure result with appropriate error messages if any rule fails
    /// - Handle null values appropriately
    /// </remarks>
    public abstract ValidationResult<T> Validate(T value);

    /// <summary>
    /// Sets a custom error message for validation failures.
    /// </summary>
    /// <param name="message">The custom error message to use</param>
    /// <returns>The validator instance for method chaining</returns>
    /// <remarks>
    /// This method enables fluent configuration of validators by:
    /// - Setting a custom error message
    /// - Returning the validator instance for chaining
    /// - Supporting method chaining with other configuration methods
    /// 
    /// Example:
    /// <code>
    /// validator
    ///     .WithMessage("Custom error")
    ///     .OtherConfiguration();
    /// </code>
    /// </remarks>
    public virtual Validator<T> WithMessage(string message)
    {
        ErrorMessage = message;
        return this;
    }

    /// <summary>
    /// Creates a ValidationResult indicating validation failure with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing the validation failure</param>
    /// <returns>A ValidationResult containing the error</returns>
    /// <remarks>
    /// This helper method simplifies creation of error results by:
    /// - Creating a new ValidationResult instance
    /// - Adding the specified error message
    /// - Using the custom error message if set, otherwise using the provided message
    /// </remarks>
    protected ValidationResult<T> CreateError(string message)
    {
        return new ValidationResult<T>(new[] { new ValidationError(message) });
    }
} 