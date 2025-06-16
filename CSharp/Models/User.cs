using System.Collections.Generic;

namespace SchemaValidation.Models;

public sealed record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required double Age { get; init; }
    public required bool IsActive { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required Address Address { get; init; }
} 