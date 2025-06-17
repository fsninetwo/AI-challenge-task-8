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

    public new StringValidator WithMessage(string message)
    {
        base.WithMessage(message);
        _patternErrorMessage = message;
        return this;
    }

    public override ValidationResult<string> Validate(string value)
    {
        if (value == null)
        {
            return CreateError(ValidationMessages.NullValue);
        }

        if (string.IsNullOrEmpty(value))
        {
            return CreateError(_customMessage ?? ValidationMessages.EmptyValue);
        }

        if (_minLength.HasValue && value.Length < _minLength.Value)
        {
            return CreateError(_customMessage ?? ValidationMessages.MinLength(_minLength.Value));
        }

        if (_maxLength.HasValue && value.Length > _maxLength.Value)
        {
            return CreateError(_customMessage ?? ValidationMessages.MaxLength(_maxLength.Value));
        }

        if (_pattern != null && !Regex.IsMatch(value, _pattern))
        {
            return CreateError(_patternErrorMessage ?? ValidationMessages.InvalidPattern(_pattern));
        }

        return ValidationResult.Success<string>();
    }
} 