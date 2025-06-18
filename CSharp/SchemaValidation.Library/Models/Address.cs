namespace SchemaValidation.Library.Models;

/// <summary>
/// Represents a physical address with standard address components.
/// This record class provides an immutable representation of address data.
/// </summary>
/// <remarks>
/// The Address model includes all standard components of a postal address:
/// - Street address
/// - City
/// - Postal code
/// - Country
/// 
/// All properties are required and must be initialized during object creation.
/// </remarks>
public sealed record Address
{
    /// <summary>
    /// Gets the street address including house/building number.
    /// </summary>
    /// <remarks>Required. Must be at least 5 characters long.</remarks>
    public required string Street { get; init; }

    /// <summary>
    /// Gets the city or town name.
    /// </summary>
    /// <remarks>Required. Must not be empty.</remarks>
    public required string City { get; init; }

    /// <summary>
    /// Gets the postal code or ZIP code.
    /// </summary>
    /// <remarks>Required. Must be exactly 5 digits.</remarks>
    public required string PostalCode { get; init; }

    /// <summary>
    /// Gets the country name.
    /// </summary>
    /// <remarks>
    /// Required. Must not be empty.
    /// When set to "USA", additional validation rules apply to the associated user's phone number.
    /// </remarks>
    public required string Country { get; init; }
} 