namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for OData entities.
    /// </summary>
    internal static class ODataEntityExtensions
    {
        internal static IEnumerable<object> WrapIntoEnumerable(this object @object)
        {
            if (@object != null && !typeof(IEnumerable<object>).IsAssignableFrom(@object.GetType()))
            {
                var list = (IEnumerable<object>)Activator.CreateInstance(typeof(List<>).MakeGenericType(@object.GetType()));
                list.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, list, new[] { @object });
                return list;
            }

            return (IEnumerable<object>)@object;
        }

        internal static Dictionary<string, string> DataServiceKeys(this object @object)
        {
            var elementType = @object.GetType();
            var dataServiceKeysAttributes = elementType.GetAttribute<DataServiceKeyAttribute>();

            return dataServiceKeysAttributes.KeyNames
                                            .Select(elementType.GetProperty)
                                            .ToDictionary(p => p.Name, p => p.GetValue(@object, null).ToString());
        }

        internal static T GetAttribute<T>(this MethodInfo @object)
        {
            return @object.GetCustomAttributes(typeof(T), true)
                          .Cast<T>()
                          .FirstOrDefault();
        }

        internal static T GetAttribute<T>(this Type @type)
        {
            return @type.GetAttributes<T>().FirstOrDefault();
        }

        internal static IEnumerable<T> GetAttributes<T>(this Type @type)
        {
            return @type.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        internal static T GetAttribute<T>(this PropertyInfo @object)
        {
            return @object.GetCustomAttributes(typeof(T), true)
                          .Cast<T>()
                          .FirstOrDefault();
        }
    }
}