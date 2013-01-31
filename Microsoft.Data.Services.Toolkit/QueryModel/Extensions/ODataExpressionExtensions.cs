namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods for the <see cref="Expression"/> class and  <see cref="LambdaExpression"/> class.
    /// </summary>
    internal static class ODataExpressionExtensions
    {
        internal static IDictionary<string, PropertyDetails> GetProjectedProperties(this LambdaExpression e)
        {
            var conditional = e.Body as ConditionalExpression;
            var init = conditional.IfFalse as MemberInitExpression;

            var sourceType = e.Type.GetGenericArguments().ElementAt(0);
            var properties = init.Bindings
                .Where(b => b.Member.Name.Equals("PropertyNameList", StringComparison.Ordinal))
                .Cast<MemberAssignment>()
                .Select(m => m.Expression)
                .Cast<ConstantExpression>()
                .Select(c => c.Value.ToString())
                .Single()
                .Split(',')
                .Select(sourceType.GetProperty);

            return properties.GetPropertyDetails();
        }

        internal static bool IsDataServiceKeyFilter(this Expression e)
        {
            var whereClause = e as UnaryExpression;
            var operand = whereClause.Operand as LambdaExpression;
            var operandType = operand.Type.GetGenericArguments().ElementAt(0);

            if (operand.Body.NodeType == ExpressionType.Equal)
            {
                var dataServiceKeyValue = operandType.GetCustomAttributes(typeof(DataServiceKeyAttribute), true)
                    .Cast<DataServiceKeyAttribute>()
                    .FirstOrDefault();

                var predicate = operand.Body as BinaryExpression;

                if (!(predicate.Left is MemberExpression))
                    return false;

                var requestedMember = (predicate.Left as MemberExpression).Member.Name;

                if (dataServiceKeyValue != null)
                    return dataServiceKeyValue.KeyNames.Contains(requestedMember);

                return requestedMember.Equals("ID", StringComparison.InvariantCulture) ||
                       requestedMember.Equals(operandType.Name + "ID", StringComparison.InvariantCulture);
            }

            return false;
        }

        internal static Type OperandType(this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            var operand = unaryExpression.Operand as LambdaExpression;
            return operand.Type.GetGenericArguments().ElementAt(0);
        }

        internal static string RightConstantExpression(this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            var operand = unaryExpression.Operand as LambdaExpression;
            var predicate = operand.Body as BinaryExpression;
            return (predicate.Right as ConstantExpression).Value.ToString();
        }

        internal static string LeftPropertyName(this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            var operand = unaryExpression.Operand as LambdaExpression;
            var predicate = operand.Body as BinaryExpression;
            return (predicate.Left as MemberExpression).Member.Name;
        }
    }
}