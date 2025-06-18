using System.Collections.Generic;
using SchemaValidation.Library.Models;

namespace SchemaValidation.Tests.Base
{
    public abstract class ValidationTestBase
    {
        protected static User CreateValidUser()
        {
            return new User
            {
                Id = "1",
                Name = "John Doe",
                Email = "john.doe@example.com",
                Age = 30,
                IsActive = true,
                PhoneNumber = "+1234567890",
                Tags = new List<string> { "tag1", "tag2" },
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    PostalCode = "12345",
                    Country = "USA"
                }
            };
        }

        protected Address CreateValidAddress()
        {
            return new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "12345",
                Country = "USA"
            };
        }

        protected User CreateInvalidUser()
        {
            return new User
            {
                Id = "123",
                Name = "A", // Invalid - too short
                Email = "invalid-email", // Invalid format
                Age = 150, // Invalid - too high
                IsActive = true,
                Tags = new List<string> { "tag1", "tag2" },
                PhoneNumber = "123-456-7890",
                Address = new Address
                {
                    Street = "St", // Invalid - too short
                    City = "Anytown",
                    PostalCode = "123", // Invalid format
                    Country = "USA"
                }
            };
        }
    }
} 