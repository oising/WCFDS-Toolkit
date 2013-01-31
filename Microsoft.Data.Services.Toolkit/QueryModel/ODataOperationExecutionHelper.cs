namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Provides methods to execute Linq2Objects operations.
    /// </summary>
    internal static class ODataOperationExecutionHelper
    {
        /// <summary>
        /// Executes a Linq2Objects call to the Skip and Take methods to a given <see cref="IEnumerable"/> object.
        /// </summary>
        /// <param name="methodName">A string that indicates the name of the method.</param>
        /// <param name="enumerable">An <see cref="IEnumerable"/> object that will be filtered.</param>
        /// <param name="count">An integer value that specifies the Skip/Take value.</param>
        /// <returns>A filtered <see cref="IEnumerable"/> object.</returns>
        public static object ExecuteLinq2ObjectsImplementation(string methodName, IEnumerable<object> enumerable, int count)
        {
            var method = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                           .Where(mi => mi.Name == methodName)
                                           .First();

            var originalType = enumerable.GetType().IsArray ? enumerable.GetType().GetElementType() : enumerable.GetType().GetGenericArguments().ElementAt(0);
            return method.MakeGenericMethod(originalType).Invoke(enumerable, new object[] { enumerable, count });
        }

        /// <summary>
        /// Executes a Linq2Objects expression to a given <see cref="IEnumerable"/> object.
        /// </summary>
        /// <param name="methodName">A string that indicates the name of the method.</param>
        /// <param name="enumerable">An <see cref="IEnumerable"/> object that will be filtered.</param>
        /// <param name="expression">An <see cref="Expression"/> to be applied to the <see cref="IEnumerable"/> object.</param>
        /// <returns>A filtered <see cref="IEnumerable"/> object.</returns>
        public static object ExecuteLinq2ObjectsImplementation(string methodName, IEnumerable<object> enumerable, Expression expression)
        {
            var orderByClause = expression as UnaryExpression;
            var operand = orderByClause == null ? expression as LambdaExpression : orderByClause.Operand as LambdaExpression;

            var whereInfo = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                              .Where(mi => mi.Name == methodName && mi.GetParameters()[1].ParameterType.GetGenericArguments().Count() == 2)
                                              .First();

            var currentType = enumerable.GetType();
            var seedElementType = currentType.IsArray ? currentType.GetElementType() : currentType.GetGenericArguments().ElementAt(0);

            var genericArguments = new List<Type> { seedElementType };

            if (whereInfo.GetGenericArguments().Count() > 1)
                genericArguments.Add(operand.Body.Type);

            var orderByMethod = whereInfo.MakeGenericMethod(genericArguments.ToArray());
            return orderByMethod.Invoke(enumerable, new object[] { enumerable, operand.Compile() });
        }

        /// <summary>
        /// Performs a ToArray call to a given object.
        /// </summary>
        /// <param name="result">The object to be converted as an array.</param>
        /// <param name="elementType">The <see cref="Type"/> of the returned Enumerable element.</param>
        /// <returns>A new array of the provided element type.</returns>
        public static object EvalEnumerable(object result, Type elementType)
        {
            var method = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
            return method.Invoke(result, new[] { result });
        }

        /// <summary>
        /// Executes the ExecuteProjectionTyped method to a given <see cref="IEnumerable"/> object.
        /// </summary>
        /// <param name="operation">The <see cref="ODataQueryOperation"/> value.</param>
        /// <param name="enumerable">An <see cref="IEnumerable"/> object.</param>
        /// <returns>The projected result of the projection.</returns>
        public static IEnumerable ExecuteProjection(ODataQueryOperation operation, IEnumerable<object> enumerable)
        {
            var executorType = typeof(ODataOperationExecutionHelper);
            var projectionMethod = executorType.GetMethod("ExecuteProjectionTyped", BindingFlags.Public | BindingFlags.Static);
            var genericMethod = projectionMethod.MakeGenericMethod(operation.ProjectedType);
            return (IEnumerable)genericMethod.Invoke(executorType, new object[] { operation.ProjectionExpression, enumerable });
        }

        /// <summary>
        /// Executes the ExecuteProjectionTyped method for a given <see cref="LambdaExpression"/> to an <see cref="IEnumerable"/> object.
        /// </summary>
        /// <param name="expression">The <see cref="LambdaExpression"/> value.</param>
        /// <param name="enumerable">An <see cref="IEnumerable"/> object.</param>
        /// <typeparam name="T">The type of the <see cref="IEnumerable"/> object.</typeparam>
        /// <returns>The projected result of the projection.</returns>
        public static IEnumerable<T> ExecuteProjectionTyped<T>(LambdaExpression expression, IEnumerable<object> enumerable)
        {
            var projectionMethod = expression.Compile();
            return enumerable.Select(item => (T)projectionMethod.DynamicInvoke(item));
        }
    }
}
