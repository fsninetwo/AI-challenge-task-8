using System;
using System.Collections.Generic;
using SchemaValidation.Core;
using SchemaValidation.Models;
using SchemaValidation.Validators;

namespace SchemaValidation
{
    public class Program
    {
        public static void Main()
        {
            // Define address schema
            var addressSchema = Schema.Object<Address>(new Dictionary<string, Validator<object>>
            {
                { "Street", Schema.String() },
                { "City", Schema.String() },
                { "PostalCode", Schema.String().Pattern(@"^\d{5}$").WithMessage("Postal code must be 5 digits") },
                { "Country", Schema.String() }
            });

            // Define user schema
            var userSchema = Schema.Object<User>(new Dictionary<string, Validator<object>>
            {
                { "Id", Schema.String().WithMessage("ID must be a string") },
                { "Name", Schema.String().MinLength(2).MaxLength(50) },
                { "Email", Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$") },
                { "Age", Schema.Number() },
                { "IsActive", Schema.Boolean() },
                { "Tags", Schema.Array(Schema.String()) }
            });

            Console.WriteLine("Test Case 1: Valid User");
            var validUser = new User
            {
                Id = "12345",
                Name = "John Doe",
                Email = "john@example.com",
                Age = 30,
                IsActive = true,
                Tags = new List<string> { "developer", "designer" },
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    PostalCode = "12345",
                    Country = "USA"
                }
            };
            ValidateAndPrint(userSchema, validUser);

            Console.WriteLine("\nTest Case 2: Invalid Email");
            var invalidEmailUser = new User
            {
                Id = "12345",
                Name = "John Doe",
                Email = "invalid-email",
                Age = 30,
                IsActive = true,
                Tags = new List<string> { "developer", "designer" }
            };
            ValidateAndPrint(userSchema, invalidEmailUser);

            Console.WriteLine("\nTest Case 3: Invalid Name Length");
            var invalidNameUser = new User
            {
                Id = "12345",
                Name = "J",  // Too short
                Email = "john@example.com",
                Age = 30,
                IsActive = true,
                Tags = new List<string> { "developer", "designer" }
            };
            ValidateAndPrint(userSchema, invalidNameUser);

            Console.WriteLine("\nTest Case 4: Invalid Postal Code");
            var invalidAddress = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "123",  // Not 5 digits
                Country = "USA"
            };
            ValidateAndPrint(addressSchema, invalidAddress);
        }

        private static void ValidateAndPrint<T>(Validator<T> schema, T data)
        {
            var result = schema.Validate(data);
            Console.WriteLine($"Validation result: {(result.IsValid ? "Valid" : "Invalid")}");
            if (!result.IsValid)
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
} 