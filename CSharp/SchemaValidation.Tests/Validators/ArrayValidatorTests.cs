using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Models;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class ArrayValidatorTests : ValidationTestBase
{
    private readonly ArrayValidator _validator;
    private readonly ArrayValidator<int> _intValidator;
    private readonly ArrayValidator<string> _stringValidator;

    public ArrayValidatorTests()
    {
        _validator = new ArrayValidator();
        _intValidator = new ArrayValidator<int>(new NumberValidator());
        _stringValidator = new ArrayValidator<string>(new StringValidator());
    }

    [Fact]
    public void Validate_WhenValueIsArray_ReturnsTrue()
    {
        // Arrange
        var value = new[] { 1, 2, 3 };

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenValueIsList_ReturnsTrue()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3 };

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("not an array")]
    [InlineData(123)]
    [InlineData(null)]
    public void Validate_WhenValueIsNotEnumerable_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(1, 1)]
    public void Validate_WhenLengthIsInRange_ReturnsTrue(int minLength, int maxLength)
    {
        // Arrange
        var value = new List<int>();
        for (int i = 0; i < minLength; i++)
        {
            value.Add(i);
        }

        _validator.MinLength(minLength);
        _validator.MaxLength(maxLength);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(3, 5, 1)]  // Too few items
    [InlineData(3, 5, 7)]  // Too many items
    public void Validate_WhenLengthIsOutOfRange_ReturnsFalse(int minLength, int maxLength, int actualLength)
    {
        // Arrange
        var value = new List<int>();
        for (int i = 0; i < actualLength; i++)
        {
            value.Add(i);
        }

        _validator.MinLength(minLength);
        _validator.MaxLength(maxLength);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var value = new List<int>();
        var customMessage = "Array must have at least 3 items";

        _validator.MinLength(3).WithMessage(customMessage);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message == customMessage);
    }

    [Fact]
    public void Validate_WithItemValidator_ValidatesEachItem()
    {
        // Arrange
        var numberValidator = new NumberValidator().Min(0).Max(10);
        _validator.Items(numberValidator);

        var validValue = new[] { 1, 5, 10 };
        var invalidValue = new[] { -1, 5, 11 };

        // Act
        var validResult = _validator.Validate(validValue);
        var invalidResult = _validator.Validate(invalidValue);

        // Assert
        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.Errors);

        Assert.False(invalidResult.IsValid);
        Assert.NotEmpty(invalidResult.Errors);
    }

    [Fact]
    public void MinLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must contain at least 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MaxLength(3));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MaxLength(2));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array cannot contain more than 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void Unique_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).Unique());

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Unique_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).Unique());

        // Act
        var result = validator.Validate(new[] { "item1", "item1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have unique items", result.Errors[0].Message);
    }

    [Fact]
    public void UniqueBy_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator())
                .UniqueBy((x, y) => x.ToLower() == y.ToLower()));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UniqueBy_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator())
                .UniqueBy((x, y) => x.ToLower() == y.ToLower()));

        // Act
        var result = validator.Validate(new[] { "item1", "ITEM1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have unique items", result.Errors[0].Message);
    }

    [Fact]
    public void ItemValidation_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator().MinLength(3)));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ItemValidation_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator().MinLength(10)));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Minimum length"));
    }

    [Fact]
    public void CombinedValidation_ShouldWork()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator().MinLength(3))
                .MinLength(2)
                .MaxLength(4)
                .Unique());

        // Act & Assert
        var validResult = validator.Validate(new[] { "item1", "item2", "item3" });
        Assert.True(validResult.IsValid);

        var tooShortResult = validator.Validate(new[] { "item1" });
        Assert.False(tooShortResult.IsValid);
        Assert.Contains("Array must contain at least 2 items", tooShortResult.Errors[0].Message);

        var tooLongResult = validator.Validate(new[] { "item1", "item2", "item3", "item4", "item5" });
        Assert.False(tooLongResult.IsValid);
        Assert.Contains("Array cannot contain more than 4 items", tooLongResult.Errors[0].Message);

        var notUniqueResult = validator.Validate(new[] { "item1", "item1", "item2" });
        Assert.False(notUniqueResult.IsValid);
        Assert.Contains("Array must have unique items", notUniqueResult.Errors[0].Message);

        var invalidItemResult = validator.Validate(new[] { "item1", "ab" });
        Assert.False(invalidItemResult.IsValid);
        Assert.Contains(invalidItemResult.Errors, e => e.Message.Contains("Minimum length"));
    }

    [Fact]
    public void Tags_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var user = CreateValidUser() with { Tags = new List<string> { "tag", "tag" } };

        // Act
        var result = UserSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Tags));
    }

    [Fact]
    public void ObjectArrayValidation_ShouldWork()
    {
        // Arrange
        var user1 = CreateValidUser();
        var user2 = CreateValidUser();

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) },
                { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")) }
            }));

        // Act & Assert
        var validResult = validator.Validate(new[] { user1, user2 });
        Assert.True(validResult.IsValid);

        user2 = user2 with { Email = "invalid-email" };
        var invalidResult = validator.Validate(new[] { user1, user2 });
        Assert.False(invalidResult.IsValid);
        Assert.NotNull(invalidResult.Errors);
        Assert.Contains(invalidResult.Errors, e => e.PropertyName?.Contains($"[1].{nameof(User.Email)}") ?? false);
    }

    [Fact]
    public void MinLength_WithNullValue_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        IEnumerable<string>? nullArray = null;
        var result = validator.Validate(nullArray!);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains("Value cannot be null", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithEmptyArray_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MaxLength(2));

        // Act
        var result = validator.Validate(Array.Empty<string>());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Unique_WithNullValues_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).Unique());

        // Act
        var array = new[] { "item1", "item2", "item1" };
        var result = validator.Validate(array);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have unique items", result.Errors[0].Message);
    }

    [Fact]
    public void UniqueBy_WithCustomComparer_ShouldWork()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<User>, object, ArrayValidator<User>>(
            new ArrayValidator<User>(new ObjectValidator<User>(new Dictionary<string, Validator<object>>()))
                .UniqueBy((u1, u2) => string.Equals(u1?.Id, u2?.Id, StringComparison.Ordinal)));

        var user1 = CreateValidUser() with { Id = "1", Name = "John" };
        var user2 = CreateValidUser() with { Id = "1", Name = "Jane" }; // Same ID, different name
        var user3 = CreateValidUser() with { Id = "2", Name = "John" }; // Different ID, same name

        // Act & Assert
        var validResult = validator.Validate(new[] { user1, user3 });
        Assert.True(validResult.IsValid);

        var invalidResult = validator.Validate(new[] { user1, user2 });
        Assert.False(invalidResult.IsValid);
        Assert.NotNull(invalidResult.Errors);
        Assert.Contains("Array must have unique items", invalidResult.Errors[0].Message);
    }

    [Fact]
    public void MinLength_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var customMessage = "Array must contain at least 3 items";
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(3, customMessage));

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(customMessage, result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        var customMessage = "Array cannot contain more than 2 items";
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MaxLength(2, customMessage));

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(customMessage, result.Errors[0].Message);
    }

    [Fact]
    public void ItemValidation_WithNullItem_ShouldFail()
    {
        // Arrange
        var stringValidator = new StringValidator();
        stringValidator.MinLength(3);
        var arrayValidator = new ArrayValidator<string>(stringValidator);
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(arrayValidator);

        // Act
        var array = new[] { "item1", "item2", "ab" };
        var result = validator.Validate(array);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Minimum length"));
    }

    [Fact]
    public void CombinedValidation_WithEdgeCases_ShouldWork()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator().MinLength(3))
                .MinLength(2)
                .MaxLength(4)
                .Unique());

        // Act & Assert
        var validResult = validator.Validate(new[] { "item1", "item2", "item3" });
        Assert.True(validResult.IsValid);

        var tooShortResult = validator.Validate(new[] { "item1" });
        Assert.False(tooShortResult.IsValid);
        Assert.Contains("Array must contain at least 2 items", tooShortResult.Errors[0].Message);

        var tooLongResult = validator.Validate(new[] { "item1", "item2", "item3", "item4", "item5" });
        Assert.False(tooLongResult.IsValid);
        Assert.Contains("Array cannot contain more than 4 items", tooLongResult.Errors[0].Message);

        var notUniqueResult = validator.Validate(new[] { "item1", "item1", "item2" });
        Assert.False(notUniqueResult.IsValid);
        Assert.Contains("Array must have unique items", notUniqueResult.Errors[0].Message);

        var invalidItemResult = validator.Validate(new[] { "item1", "ab" });
        Assert.False(invalidItemResult.IsValid);
        Assert.Contains(invalidItemResult.Errors, e => e.Message.Contains("Minimum length"));
    }

    [Fact]
    public void UniqueBy_WithPropertyNameAndSelector_WithValidInput_ShouldPass()
    {
        // Arrange
        var user1 = CreateValidUser() with { Id = "1", Email = "test1@example.com" };
        var user2 = CreateValidUser() with { Id = "2", Email = "test2@example.com" };
        var users = new[] { user1, user2 };

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>())
                .UniqueBy(nameof(User.Email), u => u.Email));

        // Act
        var result = validator.Validate(users);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UniqueBy_WithPropertyNameAndSelector_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var user1 = CreateValidUser() with { Id = "1", Email = "test@example.com" };
        var user2 = CreateValidUser() with { Id = "2", Email = "test@example.com" };
        var users = new[] { user1, user2 };

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>())
                .UniqueBy(nameof(User.Email), u => u?.Email ?? string.Empty));

        // Act
        var result = validator.Validate(users);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
        Assert.Contains(result.Errors, e => e.Message?.Contains("Duplicate value") ?? false);
        Assert.Contains(result.Errors, e => e.Message?.Contains("test@example.com") ?? false);
    }

    [Fact]
    public void UniqueBy_WithNullPropertyName_ShouldThrowArgumentException()
    {
        // Arrange
        var validator = new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => validator.UniqueBy(null!, u => u.Email));
    }

    [Fact]
    public void UniqueBy_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var validator = new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.UniqueBy(nameof(User.Email), null!));
    }

    [Fact]
    public void ObjectArrayValidator_Unique_ShouldWork()
    {
        // Arrange
        var user1 = CreateValidUser();
        var user2 = CreateValidUser();
        var user3 = user1; // Same reference as user1

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) },
                { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")) }
            }).Unique());

        // Act & Assert
        var validResult = validator.Validate(new[] { user1, user2 });
        Assert.True(validResult.IsValid);

        var invalidResult = validator.Validate(new[] { user1, user2, user3 });
        Assert.False(invalidResult.IsValid);
        Assert.Contains(invalidResult.Errors, e => e.Message?.Contains("duplicate items") ?? false);
    }

    [Fact]
    public void ObjectArrayValidator_Unique_WithDuplicateObjects_ShouldFail()
    {
        // Arrange
        var user1 = CreateValidUser();
        var user2 = user1; // Same reference = duplicate

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }).Unique());

        // Act
        var result = validator.Validate(new[] { user1, user2 });

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Message?.Contains("duplicate items") ?? false);
    }

    [Fact]
    public void ObjectArrayValidator_Unique_WithUniqueObjects_ShouldPass()
    {
        // Arrange
        var user1 = CreateValidUser();
        var user2 = CreateValidUser(); // Different reference = unique

        var validator = new ValidatorWrapper<IEnumerable<User>, object, ObjectArrayValidator<User>>(
            new ObjectArrayValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }).Unique());

        // Act
        var result = validator.Validate(new[] { user1, user2 });

        // Assert
        Assert.True(result.IsValid);
    }
} 