using System.Collections.Generic;

namespace SchemaValidation.Models;

public sealed record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required double Age { get; init; }
    public required bool IsActive { get; init; }
    public IReadOnlyCollection<string>? Tags { get; init; }
    public string? PhoneNumber { get; init; }
    public Address? Address { get; init; }
} 