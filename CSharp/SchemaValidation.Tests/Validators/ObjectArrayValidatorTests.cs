using System;
using System.Collections.Generic;
using Xunit;
using SchemaValidation.Library.Validators;
using SchemaValidation.Tests.Base;
using SchemaValidation.Core;
using SchemaValidation.Models;

namespace SchemaValidation.Tests.Validators
{
    public class ObjectArrayValidatorTests : ValidationTestBase
    {
        private readonly ObjectArrayValidator<User> _validator;
        private readonly Dictionary<string, Validator<object>> _propertyValidators;

        public ObjectArrayValidatorTests()
        {
            _propertyValidators = new Dictionary<string, Validator<object>>
            {
                { nameof(User.Name), new StringValidator().MinLength(3) },
                { nameof(User.Age), new NumberValidator().Min(0).Max(120) },
                { nameof(User.Email), new StringValidator().Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$") },
                { nameof(User.Id), new StringValidator().MinLength(1) }
            };
            _validator = new ObjectArrayValidator<User>(_propertyValidators);
        }

        [Fact]
        public void Validate_WhenAllObjectsValid_ReturnsTrue()
        {
            // Arrange
            var objects = new[]
            {
                CreateValidUser(),
                CreateValidUser() with { Id = "456", Name = "Jane Smith", Email = "jane@example.com" }
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WhenOneObjectInvalid_ReturnsFalse()
        {
            // Arrange
            var objects = new[]
            {
                CreateValidUser(),
                CreateInvalidUser()
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(User.Name)));
        }

        [Fact]
        public void Validate_WhenArrayEmpty_ReturnsTrue()
        {
            // Arrange
            var objects = new User[] { };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithMinLength_ValidatesArrayLength()
        {
            // Arrange
            _validator.MinLength(2);
            var objects = new[]
            {
                CreateValidUser()
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message.Contains("at least"));
        }

        [Fact]
        public void Validate_WithMaxLength_ValidatesArrayLength()
        {
            // Arrange
            _validator.MaxLength(1);
            var objects = new[]
            {
                CreateValidUser(),
                CreateValidUser() with { Id = "456", Name = "Jane Smith", Email = "jane@example.com" }
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message.Contains("at most"));
        }

        [Fact]
        public void Validate_WithUnique_DetectsDuplicates()
        {
            // Arrange
            _validator.Unique();
            var user = CreateValidUser();
            var objects = new[]
            {
                user,
                user  // Same reference = duplicate
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message.Contains("unique"));
        }

        [Fact]
        public void Validate_WithUniqueBy_DetectsDuplicatesByProperty()
        {
            // Arrange
            _validator.UniqueBy(nameof(User.Email), obj => obj.Email);
            var objects = new[]
            {
                CreateValidUser(),
                CreateValidUser()  // Same email = duplicate
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(User.Email)));
        }

        [Fact]
        public void Validate_WithCustomMessage_UsesCustomMessageOnError()
        {
            // Arrange
            var customMessage = "Array must have at least 2 items";
            _validator.MinLength(2).WithMessage(customMessage);
            var objects = new[]
            {
                CreateValidUser()
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message == customMessage);
        }

        [Fact]
        public void Validate_WithNullArray_ReturnsFalse()
        {
            // Act
            var result = _validator.Validate(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_WithNullObjectInArray_ValidatesGracefully()
        {
            // Arrange
            var objects = new User[]
            {
                CreateValidUser(),
                null
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_WithMultipleValidationRules_ValidatesAll()
        {
            // Arrange
            _validator
                .MinLength(2)
                .MaxLength(3)
                .Unique()
                .WithMessage("Custom validation error");

            var objects = new[]
            {
                CreateValidUser()
            };

            // Act
            var result = _validator.Validate(objects);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }
    }
} 