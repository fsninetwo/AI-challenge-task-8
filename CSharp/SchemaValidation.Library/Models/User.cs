using System.Collections.Generic;

namespace SchemaValidation.Library.Models;

public sealed record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required int Age { get; init; }
    public required bool IsActive { get; init; }
    public Address? Address { get; init; }
    public string? PhoneNumber { get; init; }
    public List<string> Tags { get; init; } = new();
} 