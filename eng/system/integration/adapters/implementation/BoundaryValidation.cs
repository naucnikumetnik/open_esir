using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenFiscalCore.System.Integration.Adapters;

internal static class BoundaryValidation
{
    internal static T Validate<T>(T value, string dependencyName, string operationName)
    {
        if (value is null)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Serialization,
                "The dependency returned an unexpected null payload.");
        }

        var failures = new List<ValidationResult>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        ValidateNode(value, failures, visited);

        if (failures.Count == 0)
        {
            return value;
        }

        var message = string.Join(
            "; ",
            failures.Select(static failure => failure.ErrorMessage ?? "Validation failure."));

        throw new ExternalDependencyFailureException(
            dependencyName,
            operationName,
            ExternalDependencyFailureKind.Protocol,
            $"The dependency returned an invalid canonical payload: {message}");
    }

    private static void ValidateNode(
        object value,
        ICollection<ValidationResult> failures,
        ISet<object> visited)
    {
        var type = value.GetType();

        if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
        {
            return;
        }

        if (!type.IsValueType && !visited.Add(value))
        {
            return;
        }

        Validator.TryValidateObject(
            value,
            new ValidationContext(value),
            failures,
            validateAllProperties: true);

        if (value is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                if (item is not null)
                {
                    ValidateNode(item, failures, visited);
                }
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length != 0 || !property.CanRead)
            {
                continue;
            }

            var propertyType = property.PropertyType;
            if (propertyType == typeof(string) || propertyType.IsPrimitive || propertyType.IsEnum)
            {
                continue;
            }

            var propertyValue = property.GetValue(value);
            if (propertyValue is not null)
            {
                ValidateNode(propertyValue, failures, visited);
            }
        }
    }
}
