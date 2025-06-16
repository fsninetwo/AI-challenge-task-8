using System.Collections.Generic;
using SchemaValidation.Validators;

namespace SchemaValidation.Core
{
    public class Schema
    {
        public static StringValidator String() => new StringValidator();
        public static NumberValidator Number() => new NumberValidator();
        public static BooleanValidator Boolean() => new BooleanValidator();
        public static DateValidator Date() => new DateValidator();
        public static ObjectValidator<T> Object<T>(Dictionary<string, Validator<object>> schema) => new ObjectValidator<T>(schema);
        public static ArrayValidator<T> Array<T>(Validator<T> itemValidator) => new ArrayValidator<T>(itemValidator);
    }
} 