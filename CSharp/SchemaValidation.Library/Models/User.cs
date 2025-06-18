using System.Collections.Generic;

namespace SchemaValidation.Library.Models;

/// <summary>
/// Represents a user in the system with their personal information and contact details.
/// This record class provides an immutable representation of user data.
/// </summary>
/// <remarks>
/// The User model includes:
/// - Basic information (ID, name, email)
/// - Personal details (age, active status)
/// - Contact information (address, phone number)
/// - Categorization (tags)
/// 
/// All required properties must be initialized during object creation.
/// Optional properties (Address, PhoneNumber) can be null.
/// </remarks>
public sealed record User
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    /// <remarks>Required. Must not be empty.</remarks>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    /// <remarks>Required. Must be at least 2 characters long.</remarks>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    /// <remarks>Required. Must be in a valid email format.</remarks>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the user's age in years.
    /// </summary>
    /// <remarks>Required. Must be between 0 and 120.</remarks>
    public required int Age { get; init; }

    /// <summary>
    /// Gets whether the user account is active.
    /// </summary>
    /// <remarks>Required. Indicates if the user can access the system.</remarks>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets the user's address information.
    /// </summary>
    /// <remarks>Optional. Contains detailed address information if provided.</remarks>
    public Address? Address { get; init; }

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    /// <remarks>
    /// Optional. When provided:
    /// - Must be in international format (+X-XXXXXXXXXX)
    /// - Must start with +1- for USA addresses
    /// </remarks>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets the list of tags associated with the user.
    /// </summary>
    /// <remarks>
    /// Required. Initialized as an empty list.
    /// Used for categorization and grouping of users.
    /// </remarks>
    public List<string> Tags { get; init; } = new();
} 