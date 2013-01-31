namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Extension methods for the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        public static IDictionary<string, PropertyDetails> GetPropertyDetails(this IEnumerable<PropertyInfo> properties)
        {
            var projectedProperties = new Dictionary<string, PropertyDetails>();

            Action<IEnumerable<PropertyInfo>, Func<object, object>> processProperties = null;
            processProperties = delegate(IEnumerable<PropertyInfo> propertyList, Func<object, object> accessor)
            {
                if (accessor == null)
                    accessor = o => o;

                foreach (var property in propertyList)
                {
                    var local = property;
                    if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                    {
                        var propertyDetails = new PropertyDetails
                                                  {
                                                      PropertyInfo = local,
                                                      PropertySetter = (target, value) => local.SetValue(accessor(target), Convert.ChangeType(value, local.PropertyType.TypeOrElementType()), null)
                                                  };

                        projectedProperties.Add(local.GetUnderlyingColumnName(), propertyDetails);
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        var propertyDetails = new PropertyDetails
                        {
                            PropertyInfo = local,
                            PropertySetter = (target, value) => local.SetValue(accessor(target), value, null)
                        };

                        projectedProperties.Add(local.GetUnderlyingColumnName(), propertyDetails);
                    }
                    else
                        processProperties(local.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance), o => local.GetValue(accessor(o), null));
                }
            };

            processProperties(properties, null);
            return projectedProperties;
        }

        public static IDictionary<string, PropertyDetails> GetPropertyDetails(this Type type)
        {
            return GetPropertyDetails(type.GetProperties());
        }

        public static Type TypeOrElementType(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            return type.IsGenericType ? type.GetGenericArguments()[0] : type;
        }
    }
}
