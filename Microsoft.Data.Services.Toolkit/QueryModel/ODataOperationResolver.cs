namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Provides a way to get information about the repository based on a given <see cref="ODataQueryOperation"/>.
    /// </summary>
    public class ODataOperationResolver
    {
        private readonly ODataQueryOperation operation;

        /// <summary>
        /// Initializes a new instance of the ODataOperationResolver class. 
        /// </summary>
        /// <param name="operation">An <see cref="ODataQueryOperation"/>.</param>
        protected ODataOperationResolver(ODataQueryOperation operation)
        {
            this.operation = operation;
        }

        /// <summary>
        /// A fluent interface to resolve repository information.
        /// </summary>
        /// <param name="operation">An <see cref="ODataQueryOperation"/>.</param>
        /// <returns>A new instance of the <see cref=" ODataOperationResolver"/> class.</returns>
        public static ODataOperationResolver For(ODataQueryOperation operation)
        {
            return new ODataOperationResolver(operation);
        }

        /// <summary>
        /// Resolve the corresponding repository.
        /// </summary>
        /// <param name="resolver">A functional to determine the repository.</param>
        /// <returns>A new repository instance.</returns>
        public object Repository(Func<string, object> resolver)
        {
            var selectMany = this.operation as ODataSelectManyQueryOperation;
            var repositoryType = this.operation.OfType;

            if (selectMany != null)
            {
                var navigationProperty = selectMany.OfType.GetProperty(selectMany.NavigationProperty);
                var counter = this.operation.IsCountRequest && navigationProperty.GetAttribute<ForeignPropertyCountAttribute>() != null;

                if (counter || navigationProperty.GetAttribute<ForeignPropertyAttribute>() != null)
                    repositoryType = selectMany.NavigationPropertyType;
            }

            var repository = resolver.Invoke(repositoryType.FullName);

            if (repository == null)
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, "No repository found for {0}.", repositoryType.Name));

            return repository;
        }

        /// <summary>
        /// Determines which repository method should be called.
        /// </summary>
        /// <param name="repository">A repository instance.</param>
        /// <returns>The <see cref="MethodInfo"/> to be executed.</returns>
        public MethodInfo Method(object repository)
        {
            var selectMany = this.operation as ODataSelectManyQueryOperation;
            var methodName = default(string);

            if (selectMany != null)
            {
                var property = selectMany.NavigationPropertyInfo;

                if (selectMany.IsCountRequest && property.GetAttribute<ForeignPropertyCountAttribute>() != null)
                {
                    var temp = repository.GetType().GetMethod(this.GetMethodName<ForeignPropertyCountAttribute>(property));

                    if (selectMany.FilterExpression == null || repository.GetBehavior(temp).HandlesFilter)
                        methodName = this.GetMethodName<ForeignPropertyCountAttribute>(property);
                }

                if (methodName == null && property.GetAttribute<ForeignPropertyAttribute>() != null)
                    methodName = this.GetMethodName<ForeignPropertyAttribute>(property);
            }

            if (methodName == null)
            {
                if (this.operation.IsCountRequest && this.operation.OfType.GetAttribute<CollectionCountAttribute>() != null)
                    methodName = this.GetMethodName<CollectionCountAttribute>(this.operation.OfType);

                if (this.operation is ODataSelectOneQueryOperation)
                    methodName = "GetOne";
            }

            methodName = methodName ?? "GetAll";
            MethodInfo method;

            if ((method = repository.GetType().GetMethod(methodName)) == null)
                throw new MissingMethodException(
                    string.Format(
                                  CultureInfo.InvariantCulture,
                                  "The method {0} cannot be found on the repository {1}.",
                                  methodName,
                                  repository.GetType().Name));

            return method;
        }

        /// <summary>
        /// Determines the arguments that will be passed to the repository method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/> where the arguments will provided.</param>
        /// <returns>An object[] containing the method arguments.</returns>
        public object[] Arguments(MethodInfo method)
        {
            if (method.GetParameters().Count() < 1)
                return null;

            if (method.GetParameters().First().ParameterType.IsAssignableFrom(this.operation.GetType()))
                return new[] { this.operation };

            return method.GetParameters()
                         .OrderBy(p => p.Position)
                         .Select(ParameterSelector(operation))
                         .ToArray();
        }

        /// <summary>
        /// Determines which repository method will be called.
        /// </summary>
        /// <typeparam name="T">The type of the navigation property attribute.</typeparam>
        /// <param name="property">The <see cref="PropertyInfo"/> marked with the <see cref="RepositorySelectManyOperationAttribute"/>.</param>
        /// <returns>The name of the repository method to be executed.</returns>
        protected string GetMethodName<T>(PropertyInfo property) where T : RepositorySelectManyOperationAttribute
        {
            var attribute = property.GetAttribute<T>();

            if (!string.IsNullOrEmpty(attribute.RepositoryMethod))
                return attribute.RepositoryMethod;

            var verb = typeof(T) == typeof(ForeignPropertyAttribute) ? "Get" : "Count";
            var propertyName = attribute.PropertyAlias ?? property.Name;
            var parentName = attribute.ParentAlias ?? this.operation.OfType.Name;

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}By{2}", verb, propertyName, parentName);
        }

        /// <summary>
        /// Determines which repository method will be called.
        /// </summary>
        /// <typeparam name="T">The type of the class attribute.</typeparam>
        /// <param name="type">The type of the entity.</param>
        /// <returns>Gets a collection count method name.</returns>
        protected string GetMethodName<T>(Type type) where T : RepositoryOperationAttribute
        {
            var attribute = type.GetAttribute<T>();

            if (!string.IsNullOrEmpty(attribute.RepositoryMethod))
                return attribute.RepositoryMethod;

            return string.Format(CultureInfo.InvariantCulture, "{0}Count", this.operation.OfType.Name);
        }

        private static Func<ParameterInfo, string> ParameterSelector(ODataQueryOperation operation)
        {
            return parameter =>
            {
                var selectOneOperation = operation as ODataSelectOneQueryOperation;
                if (selectOneOperation != null)
                {
                    // Pass #1: Check for a single ID property.
                    if (parameter.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                        return selectOneOperation.Key;

                    // Pass #2: Check for a key value.
                    if (selectOneOperation.Keys.Keys.SingleOrDefault(k => k.Equals(parameter.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
                        return selectOneOperation.Keys.SingleOrDefault(o => o.Key.Equals(parameter.Name, StringComparison.InvariantCultureIgnoreCase)).Value;
                }

                // Pass #3: Check for a context parameter.
                return operation.ContextParameters.SingleOrDefault(d => d.Key.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)).Value;
            };
        }
    }
}