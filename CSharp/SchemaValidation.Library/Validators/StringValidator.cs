using System;
using System.Text.RegularExpressions;
using SchemaValidation.Core;

namespace SchemaValidation.Library.Validators;

public sealed class StringValidator : Validator<string>
{
    private int? _minLength;
    private int? _maxLength;
    private string? _pattern;
    private string? _patternErrorMessage;

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
        _patternErrorMessage = "Email format is invalid";
        return this;
    }

    public StringValidator PhoneNumber()
    {
        _pattern = @"^\d{3}-\d{3}-\d{4}$";
        _patternErrorMessage = "Phone number format is invalid (expected format: XXX-XXX-XXXX)";
        return this;
    }

    public new StringValidator WithMessage(string message)
    {
        base.WithMessage(message);
        _patternErrorMessage = message;
        return this;
    }

    public override ValidationResult<string> Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (_minLength.HasValue && value.Length < _minLength.Value)
            return CreateError($"Minimum length is {_minLength.Value}");

        if (_maxLength.HasValue && value.Length > _maxLength.Value)
            return CreateError($"Maximum length is {_maxLength.Value}");

        if (_pattern is not null && !Regex.IsMatch(value, _pattern))
            return CreateError(_patternErrorMessage ?? "Pattern validation failed");

        return ValidationResult.Success<string>();
    }
} 