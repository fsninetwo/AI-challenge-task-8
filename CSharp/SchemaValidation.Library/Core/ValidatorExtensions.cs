using System;
using System.Collections.Generic;
using SchemaValidation.Validators;

namespace SchemaValidation.Core;

public static class ValidatorExtensions
{
    public static Validator<T> Optional<T>(this Validator<T> validator, string propertyName)
    {
        // Mark the property as optional in the validator
        if (validator is ObjectValidator<T> objectValidator)
        {
            objectValidator.MarkPropertyAsOptional(propertyName);
        }
        return validator;
    }

    public static ConditionalValidator<T> When<T>(this Validator<T> validator, string propertyName, Func<T, bool> condition)
    {
        return new ConditionalValidator<T>(validator, propertyName, condition);
    }
}

public class ConditionalValidator<T>
{
    private readonly Validator<T> _validator;
    private readonly string _propertyName;
    private readonly Func<T, bool> _condition;

    public ConditionalValidator(Validator<T> validator, string propertyName, Func<T, bool> condition)
    {
        _validator = validator;
        _propertyName = propertyName;
        _condition = condition;
    }

    public void DependsOn<TProperty1, TProperty2>(string property1Name, string property2Name, Func<TProperty1, TProperty2, bool> dependencyRule)
    {
        if (_validator is ObjectValidator<T> objectValidator)
        {
            objectValidator.AddDependencyRule(_propertyName, property1Name, property2Name, (obj, prop1, prop2) =>
            {
                if (!_condition(obj))
                {
                    return true; // Skip validation if condition is not met
                }

                if (prop1 is TProperty1 p1 && prop2 is TProperty2 p2)
                {
                    return dependencyRule(p1, p2);
                }

                return false; // Validation fails if types don't match
            });
        }
    }
} 