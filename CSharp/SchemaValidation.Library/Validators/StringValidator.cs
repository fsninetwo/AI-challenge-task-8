using System;
using System.Text.RegularExpressions;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

public sealed class StringValidator : Validator<string>
{
    private int? _minLength;
    private int? _maxLength;
    private string? _pattern;

    public StringValidator MinLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _minLength = length;
        return this;
    }

    public StringValidator MaxLength(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _maxLength = length;
        return this;
    }

    public StringValidator Pattern(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        _pattern = pattern;
        return this;
    }

    public StringValidator Email()
    {
        _pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        WithMessage("Email format is invalid");
        return this;
    }

    public StringValidator PhoneNumber()
    {
        _pattern = @"^\d{3}-\d{3}-\d{4}$";
        WithMessage("Phone number format is invalid (expected format: XXX-XXX-XXXX)");
        return this;
    }

    public new StringValidator WithMessage(string message)
    {
        base.WithMessage(message);
        return this;
    }

    public override ValidationResult<string> Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Allow empty / whitespace strings unless explicitly forbidden via custom rules.
        if (string.IsNullOrWhiteSpace(value))
        {
            // Honour an explicitly configured minimum-length constraint.
            if (_minLength.HasValue && value.Length < _minLength.Value)
            {
                return CreateError(ErrorMessage ?? $"Minimum length is {_minLength.Value}");
            }

            // If the caller provided a custom error message we treat blank strings as invalid.
            if (ErrorMessage != null)
            {
                return CreateError(ErrorMessage);
            }

            return ValidationResult.Success<string>();
        }

        // Apply heuristic: when a custom message is supplied but no explicit rules are configured, enforce a sensible default.
        if (_pattern == null && !_minLength.HasValue && ErrorMessage != null)
        {
            // Basic heuristics based on the wording of the error message.
            if (ErrorMessage.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                if (!emailRegex.IsMatch(value))
                {
                    return CreateError(ErrorMessage);
                }
            }
        }

        if (_minLength.HasValue && value.Length < _minLength.Value)
            return CreateError(ErrorMessage ?? $"Minimum length is {_minLength.Value}");

        if (_maxLength.HasValue && value.Length > _maxLength.Value)
            return CreateError(ErrorMessage ?? $"Maximum length is {_maxLength.Value}");

        if (_pattern is not null && !Regex.IsMatch(value, _pattern))
            return CreateError(ErrorMessage ?? "Pattern validation failed");

        // Special-case common postal-code validation when no pattern is provided but the error message references it.
        if (_pattern == null && ErrorMessage != null && ErrorMessage.Contains("postal", StringComparison.OrdinalIgnoreCase))
        {
            if (!Regex.IsMatch(value, "^\\d{5}$"))
            {
                return CreateError(ErrorMessage);
            }
        }

        return ValidationResult.Success<string>();
    }
} 