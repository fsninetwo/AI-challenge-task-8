using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using Xunit;

namespace SchemaValidation.Tests.Validators
{
    public class BooleanValidatorTests
    {
        private readonly Validator<object> _validator;
        private readonly BooleanValidator _underlyingValidator;

        public BooleanValidatorTests()
        {
            _validator = Schema.Boolean();
            _underlyingValidator = ((SchemaValidation.Core.ValidatorWrapper<bool, object, BooleanValidator>)_validator).UnderlyingValidator;
        }

        [Fact]
        public void Validate_WhenValueIsBoolean_ReturnsTrue()
        {
            // Arrange
            var value = true;

            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(1)]
        [InlineData("true")]
        [InlineData(null)]
        public void Validate_WhenValueIsNotBoolean_ReturnsFalse(object value)
        {
            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Message.Contains("Value must be a boolean"));
        }

        [Fact]
        public void Validate_WithCustomMessage_UsesCustomMessageOnError()
        {
            // Arrange
            var customMessage = "Must be a valid boolean value";
            _validator.WithMessage(customMessage);

            // Act
            var result = _validator.Validate("not a boolean");

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message == customMessage);
        }
    }
} 