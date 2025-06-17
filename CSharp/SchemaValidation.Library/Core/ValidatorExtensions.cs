using System;
using System.Collections.Generic;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Core;

public static class ValidatorExtensions
{
    public static ObjectValidator<T> Optional<T>(this ObjectValidator<T> validator, string propertyName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        validator.MarkPropertyAsOptional(propertyName);
        return validator;
    }

    public static DependencyBuilder<T> When<T>(this ObjectValidator<T> validator, string propertyName, Func<T, bool> condition) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(condition);
        return new DependencyBuilder<T>(validator, propertyName, condition);
    }
}

public sealed class DependencyBuilder<T> where T : class
{
    private readonly ObjectValidator<T> _validator;
    private readonly string _propertyName;
    private readonly Func<T, bool> _condition;

    public DependencyBuilder(ObjectValidator<T> validator, string propertyName, Func<T, bool> condition)
    {
        _validator = validator;
        _propertyName = propertyName;
        _condition = condition;
    }

    public ObjectValidator<T> DependsOn<TProperty1, TProperty2>(string property1Name, string property2Name, Func<TProperty1, TProperty2, bool> rule)
    {
        ArgumentException.ThrowIfNullOrEmpty(property1Name);
        ArgumentException.ThrowIfNullOrEmpty(property2Name);
        ArgumentNullException.ThrowIfNull(rule);

        _validator.AddDependencyRule(_propertyName, property1Name, property2Name, 
            (obj, prop1, prop2) => !_condition(obj) || rule((TProperty1)prop1!, (TProperty2)prop2!));

        return _validator;
    }
} 