# Schema Validation Library for .NET

A lightweight and extensible validation library that allows you to declaratively compose validation rules for primitives, collections, and complex object graphs.

This repository contains:

* **SchemaValidation.Library** – the validation library itself.
* **SchemaValidation.Core** – shared abstractions used by the library.
* **SchemaValidation.Demo** – a console application that demonstrates real-world usage.
* **SchemaValidation.Tests** – an xUnit test-suite with **132** passing tests and ~80 % line/branch coverage.

---

## 1. Quick start

```bash
# Clone the repo
$ git clone <your-fork-url>
$ cd CSharp

# Restore, build & run the full test-suite (requires the .NET 9 SDK – Preview 9.0.100 or later)
$ dotnet restore
$ dotnet build --configuration Release
$ dotnet test CSharp/SchemaValidation.sln
```

Run the interactive demo:

```bash
# Launch the console demo which validates several User instances
dotnet run --project SchemaValidation.Demo/SchemaValidation.Demo.csproj
```

---

## 2. Using the library in your project

The library targets **netstandard2.1** so it can be referenced from .NET 6/7/8 projects.

1. Add a project reference:
   ```bash
   dotnet add <your-project>.csproj reference SchemaValidation.Library/SchemaValidation.Library.csproj
   ```
   *or* add the two `.csproj` files (**Core** and **Library**) to your solution and reference them from Visual Studio.

2. Import the namespaces you need:
   ```csharp
   using SchemaValidation.Core;          // entry-point factory – Schema.String(), Schema.Object<…>()
   using SchemaValidation.Library.Models; // your domain models (e.g. User, Address)
   using SchemaValidation.Library.Validators;
   ```

### 2.1 Validating primitive values

```csharp
var emailValidator = Schema.String()
                          .Email()                  // builtin extension
                          .WithMessage("Invalid e-mail address");

var result = emailValidator.Validate("john@example.com");
Console.WriteLine(result.IsValid); // → True
```

### 2.2 Validating complex objects

Object validation is driven by a **schema** – a `Dictionary<string, Validator<object>>` where each key maps to a property name.

```csharp
var userSchema = new Dictionary<string, Validator<object>>
{
    [nameof(User.Id)]    = Schema.String().MinLength(1),
    [nameof(User.Name)]  = Schema.String().MinLength(2),
    [nameof(User.Email)] = Schema.String().Email(),
    [nameof(User.Age)]   = Schema.Number().Range(0, 120),
    [nameof(User.Tags)]  = Schema.Array<string>(Schema.String()).MinLength(1),
};

var userValidator = new ObjectValidator<User>(userSchema);

var user = new User { Id = "1", Name = "Jane", Email = "jane@corp.com", Age = 28, IsActive = true, Tags = new() { "admin" } };
var validation = userValidator.Validate(user);

if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
```

### 2.3 Optional properties & conditional rules

```csharp
userValidator.MarkPropertyAsOptional(nameof(User.PhoneNumber));

userValidator.AddDependencyRule(
    propertyName: nameof(User.PhoneNumber),
    property1Name: nameof(User.PhoneNumber),
    property2Name: $"{nameof(User.Address)}.{nameof(Address.Country)}", // nested property path
    rule: (u, phone, country) => country?.ToString() != "USA" || (phone?.ToString()?.StartsWith("+1-") ?? false),
    message: "For US users the phone number must start with '+1-'"
);
```

---

## 3. Project layout

```
CSharp/
├── SchemaValidation.Core/         # abstract base types (Validator<>, ValidationResult<> …)
├── SchemaValidation.Library/      # concrete validators + helper extensions
├── SchemaValidation.Demo/         # console app – easiest place to play with the API
├── SchemaValidation.Tests/        # >130 unit tests powered by xUnit
└── README.md
```

---

## 4. Extending the library

Writing your own validator is straightforward:

```csharp
public sealed class GuidValidator : Validator<string>
{
    public override ValidationResult<string> Validate(string value) =>
        Guid.TryParse(value, out _) ? ValidationResult.Success<string>()
                                    : CreateError("Value must be a valid GUID");
}
```

You can then expose it through the `Schema` factory for fluent usage.

```csharp
public static class GuidSchemaExtension
{
    public static Validator<object> Guid() =>
        new ValidatorWrapper<string, object, GuidValidator>(new GuidValidator());
}
```

---

## 5. License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details. 