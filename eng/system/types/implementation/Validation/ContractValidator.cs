using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenFiscalCore.System.Types.Validation;

public static class ContractValidator
{
    public static void ValidateObjectGraph(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        ValidateNode(target, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static void ValidateNode(object target, HashSet<object> visited)
    {
        if (target is string)
        {
            return;
        }

        if (!target.GetType().IsValueType && !visited.Add(target))
        {
            return;
        }

        Validator.ValidateObject(target, new ValidationContext(target), validateAllProperties: true);

        if (target is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not null && item is not string && !item.GetType().IsValueType)
                {
                    ValidateNode(item, visited);
                }
            }

            return;
        }

        foreach (var property in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            var value = property.GetValue(target);
            if (value is null || value is string || value.GetType().IsValueType)
            {
                continue;
            }

            ValidateNode(value, visited);
        }
    }
}
