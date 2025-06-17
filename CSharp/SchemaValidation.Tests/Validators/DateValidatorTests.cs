using System;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using Xunit;

namespace SchemaValidation.Tests.Validators
{
    public class DateValidatorTests
    {
        private readonly DateValidator _validator;

        public DateValidatorTests()
        {
            _validator = new DateValidator();
        }

        [Fact]
        public void Validate_WhenValueIsDateTime_ReturnsTrue()
        {
            // Arrange
            var value = DateTime.Now;

            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("2023-01-01")]
        [InlineData(123)]
        [InlineData(null)]
        public void Validate_WhenValueIsNotDateTime_ReturnsFalse(object value)
        {
            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_WhenDateIsInRange_ReturnsTrue()
        {
            // Arrange
            var minDate = new DateTime(2023, 1, 1);
            var maxDate = new DateTime(2023, 12, 31);
            var value = new DateTime(2023, 6, 15);

            _validator.Min(minDate);
            _validator.Max(maxDate);

            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WhenDateIsBeforeMinDate_ReturnsFalse()
        {
            // Arrange
            var minDate = new DateTime(2023, 1, 1);
            var value = new DateTime(2022, 12, 31);

            _validator.Min(minDate);

            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Validate_WhenDateIsAfterMaxDate_ReturnsFalse()
        {
            // Arrange
            var maxDate = new DateTime(2023, 12, 31);
            var value = new DateTime(2024, 1, 1);

            _validator.Max(maxDate);

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
            var minDate = new DateTime(2023, 1, 1);
            var value = new DateTime(2022, 12, 31);
            var customMessage = "Date must be after January 1st, 2023";

            _validator.Min(minDate).WithMessage(customMessage);

            // Act
            var result = _validator.Validate(value);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Message == customMessage);
        }
    }
} 