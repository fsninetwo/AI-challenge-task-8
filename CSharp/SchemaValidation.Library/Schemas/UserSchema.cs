using System.Collections.Generic;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Library.Schemas;

public static class UserSchema
{
    public static ObjectValidator<User> Create()
    {
        var schema = new Dictionary<string, Validator<object>>
        {
            [nameof(User.Id)] = Schema.String().WithMessage("Id is required"),
            [nameof(User.Name)] = Schema.String().WithMessage("Name must be at least 2 characters long"),
            [nameof(User.Email)] = Schema.String().WithMessage("Invalid email format"),
            [nameof(User.Age)] = Schema.Number().WithMessage("Age must be between 0 and 120"),
            [nameof(User.IsActive)] = Schema.Boolean(),
            [nameof(User.PhoneNumber)] = Schema.String().WithMessage("Invalid phone number format"),
            [nameof(User.Tags)] = Schema.Array<string>(Schema.String().WithMessage("Tags must be unique and between 1 and 10 items"))
        };

        return new ObjectValidator<User>(schema);
    }
} 