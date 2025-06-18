using System.Collections.Generic;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Library.Schemas;

/// <summary>
/// Provides the validation schema configuration for the User model.
/// </summary>
/// <remarks>
/// This static class defines the validation rules for all User properties including:
/// - Basic validation (required fields, length constraints)
/// - Format validation (email, phone number patterns)
/// - Numeric range validation (age)
/// - Complex validation (address validation, USA phone number rules)
/// </remarks>
public static class UserSchema
{
    /// <summary>
    /// Creates a configured ObjectValidator for User objects.
    /// </summary>
    /// <returns>
    /// An ObjectValidator instance with predefined validation rules for User objects.
    /// </returns>
    /// <remarks>
    /// The validator includes the following rules:
    /// - Id: Required, non-empty string
    /// - Name: Minimum 2 characters
    /// - Email: Valid email format
    /// - Age: Between 0 and 120
    /// - IsActive: Boolean
    /// - PhoneNumber: Optional, international format (+X-XXXXXXXXXX)
    /// - Tags: Non-empty array of strings
    /// - Address: Optional, validated using AddressSchema
    /// </remarks>
    public static ObjectValidator<User> Create()
    {
        var schema = new Dictionary<string, Validator<object>>
        {
            [nameof(User.Id)] = Schema.String().WithMessage("Id is required"),
            [nameof(User.Name)] = Schema.String().WithMessage("Name must be at least 2 characters long"),
            [nameof(User.Email)] = Schema.String().WithMessage("Invalid email format"),
            [nameof(User.Age)] = Schema.Number().WithMessage("Age must be between 0 and 120"),
            [nameof(User.IsActive)] = Schema.Boolean(),
            [nameof(User.PhoneNumber)] = Schema.String().WithMessage("Invalid phone number format"),
            [nameof(User.Tags)] = Schema.Array<string>(Schema.String().WithMessage("Tags must be unique and between 1 and 10 items"))
        };

        return new ObjectValidator<User>(schema);
    }
} 