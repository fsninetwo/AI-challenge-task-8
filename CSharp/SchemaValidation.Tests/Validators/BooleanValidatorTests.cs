using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using Xunit;

namespace SchemaValidation.Tests.Validators
{
    public class BooleanValidatorTests
    {
        private readonly BooleanValidator _validator;

        public BooleanValidatorTests()
        {
            _validator = new BooleanValidator();
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
            Assert.NotEmpty(result.Errors);
        }
    }
} 