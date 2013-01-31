namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Parses the OData query expression tree and generates the proper <see cref="ODataQueryOperation"/>.
    /// </summary>
    internal class ODataExpressionVisitor : ExpressionVisitor
    {
        private readonly Queue<OperationTuple> expressions;
        private readonly Expression expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataExpressionVisitor"/> class.
        /// </summary>
        /// <param name="expression">The initial <see cref="Expression"/> to be parsed.</param>
        public ODataExpressionVisitor(Expression expression)
        {
            this.expression = expression;
            this.expressions = new Queue<OperationTuple>();
        }

        /// <summary>
        /// Evaluates the OData query <see cref="Expression"/>.
        /// </summary>
        /// <returns>The <see cref="ODataQueryOperation"/>.</returns>
        public ODataQueryOperation Eval()
        {
            this.Visit(this.expression);
            return EvalForQueue(this.expressions);
        }

        /// <summary>
        /// Parses the <see cref="MethodCallExpression"/> to enqueue a new operation 
        /// tuple based on the expression method name.
        /// </summary>
        /// <param name="node">The node to be parsed.</param>
        /// <returns>The visited node <see cref="Expression"/>.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                switch (node.Method.Name)
                {
                    case "Where":
                        this.HandleWhere(node.Arguments[1]);
                        return Visit(node.Arguments[0]);
                    case "Select":
                        this.HandleSelect(node.Arguments[1]);
                        return Visit(node.Arguments[0]);
                    case "SelectMany":
                        this.HandleSelectMany(node.Arguments[1]);
                        return Visit(node.Arguments[0]);
                    case "LongCount":
                        this.HandleCount();
                        return Visit(node.Arguments[0]);
                    case "ThenBy":
                    case "OrderBy":
                    case "ThenByDescending":
                    case "OrderByDescending":
                        this.HandleOrder(node.Arguments[1], node.Method.Name);
                        return Visit(node.Arguments[0]);
                    case "Take":
                        this.HandleTop(node.Arguments[1]);
                        return Visit(node.Arguments[0]);
                    case "Skip":
                        this.HandleSkip(node.Arguments[1]);
                        return Visit(node.Arguments[0]);
                }
            }

            throw new NotSupportedException(string.Format("Methods in type '{0}' not supported.", node.Method.DeclaringType.Name));
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'Root' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        /// <returns>The provided <see cref="ConstantExpression"/>.</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.expressions.Enqueue(new OperationTuple { Name = "Root", Expression = node });

            return node;
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'Where' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        protected virtual void HandleWhere(Expression node)
        {
            this.expressions.Enqueue(new OperationTuple { Name = "Where", Expression = node });
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'Order' value name, the given expression and the method name.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        /// <param name="methodName">The method name.</param>
        protected virtual void HandleOrder(Expression node, string methodName)
        {
            var orderExpression = new ODataOrderExpression { Expression = node, OrderMethodName = methodName };
            this.expressions.Enqueue(new OperationTuple { Name = "Order", Expression = orderExpression });
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'SelectMany' or 'Select' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        protected virtual void HandleSelect(Expression node)
        {
            var unary = node as UnaryExpression;
            var lamda = unary.Operand as LambdaExpression;
            var conditional = lamda.Body as ConditionalExpression;
            var operation = conditional == null ? "SelectMany" : "Select";
            this.expressions.Enqueue(new OperationTuple { Name = operation, Expression = node });
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'Take' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        protected virtual void HandleTop(Expression node)
        {
            this.expressions.Enqueue(new OperationTuple { Name = "Take", Expression = node });
        }
        
        /// <summary>
        /// Enqueue a new operation tuple with the 'Skip' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        protected virtual void HandleSkip(Expression node)
        {
            this.expressions.Enqueue(new OperationTuple { Name = "Skip", Expression = node });
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'SelectMany' value name and the given expression.
        /// </summary>
        /// <param name="node">The operation tuple <see cref="Expression"/>.</param>
        protected virtual void HandleSelectMany(Expression node)
        {
            this.expressions.Enqueue(new OperationTuple { Name = "SelectMany", Expression = node });
        }

        /// <summary>
        /// Enqueue a new operation tuple with the 'Count' value name.
        /// </summary>
        protected virtual void HandleCount()
        {
            this.expressions.Enqueue(new OperationTuple { Name = "Count" });
        }

        /// <summary>
        /// Evaluates the <see cref="OperationTuple"/> queue to return a proper <see cref="ODataQueryOperation"/>.
        /// </summary>
        /// <param name="tuples">The <see cref="OperationTuple"/> queue.</param>
        /// <returns>An <see cref="ODataQueryOperation"/>.</returns>
        private static ODataQueryOperation EvalForQueue(Queue<OperationTuple> tuples)
        {
            var item = tuples.Dequeue();
            var itemCount = default(OperationTuple);
            var extendedProperty = default(string);
            var filterExpression = default(Expression);
            var orderStack = new Stack<ODataOrderExpression>();
            var topCount = 0;
            var skipCount = 0;

            var projectionExpression = default(LambdaExpression);
            var projectionType = default(Type);
            IDictionary<string, PropertyDetails> projectedProperties = null;

            if (item.Name == "Select")
            {
                var unary = item.Expression as UnaryExpression;
                projectionExpression = unary.Operand as LambdaExpression;
                projectedProperties = projectionExpression.GetProjectedProperties();

                var conditional = projectionExpression.Body as ConditionalExpression;
                var init = conditional.IfFalse as MemberInitExpression;
                projectionType = init.Type;
                item = tuples.Dequeue();
            }

            if (item.Name == "Where" && item.Expression.IsDataServiceKeyFilter())
            {
                var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { item.Expression.LeftPropertyName(), item.Expression.RightConstantExpression() } };

                while (tuples.Peek().Name == "Where" && tuples.Peek().Expression.IsDataServiceKeyFilter() && tuples.Peek().Expression.OperandType() == item.Expression.OperandType())
                {
                    item = tuples.Dequeue();
                    values.Add(item.Expression.LeftPropertyName(), item.Expression.RightConstantExpression());
                }

                return new ODataSelectOneQueryOperation
                {
                    OfType = item.Expression.OperandType(),
                    Keys = values,
                    ProjectionExpression = projectionExpression,
                    ProjectedType = projectionType,
                    ProjectedProperties = projectedProperties,
                    Parent = EvalForQueue(tuples) as ODataSelectOneQueryOperation
                };
            }

            if (item.Name == "Count")
            {
                itemCount = item;
                item = tuples.Dequeue();
            }

            if (item.Name == "Take")
            {
                topCount = Convert.ToInt32((item.Expression as ConstantExpression).Value);
                item = tuples.Dequeue();
            }

            if (item.Name == "Skip")
            {
                skipCount = Convert.ToInt32((item.Expression as ConstantExpression).Value);
                item = tuples.Dequeue();
            }

            while (item.Name == "Order")
            {
                orderStack.Push(item.Expression as ODataOrderExpression);
                item = tuples.Dequeue();
            }

            // The check for !key prevents jaydata's single() function from working
            // because it issues: $filter=Id%20eq%201&$top=2
            // It does this as an optimisation - it will throw if two or more are returned 
            // so why waste the bandwidth for returning > 2
            // LOGGED: https://github.com/jaydata/jaydata/issues/71
            if (item.Name == "Where" && !item.Expression.IsDataServiceKeyFilter())
            {
                filterExpression = item.Expression;
                item = tuples.Dequeue();
            }

            if (item.Name == "SelectMany" && tuples.Peek().Name == "SelectMany")
            {
                var extendedPropertyItem = item;
                item = tuples.Dequeue();

                var extendedPropertyExp = extendedPropertyItem.Expression as UnaryExpression;
                var extendedPropertyOperand = extendedPropertyExp.Operand as LambdaExpression;
                var extendePropertyOperandBody = extendedPropertyOperand.Body;
                MemberExpression extendedPropertyMember;

                if (extendePropertyOperandBody is UnaryExpression)
                    extendedPropertyMember = (extendePropertyOperandBody as UnaryExpression).Operand as MemberExpression;
                else
                    extendedPropertyMember = extendePropertyOperandBody as MemberExpression;

                extendedProperty = extendedPropertyMember.Member.Name;
            }

            if (item.Name == "SelectMany" && tuples.Peek().Name == "Where")
            {
                var where = tuples.Dequeue();
                var values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                              {
                                { where.Expression.LeftPropertyName(),  where.Expression.RightConstantExpression() }
                              };

                while (tuples.Peek().Name == "Where" && tuples.Peek().Expression.IsDataServiceKeyFilter() && tuples.Peek().Expression.OperandType() == where.Expression.OperandType())
                {
                    where = tuples.Dequeue();
                    values.Add(where.Expression.LeftPropertyName(), where.Expression.RightConstantExpression());
                }

                var navPropClause = item.Expression as UnaryExpression;
                var operand = navPropClause.Operand as LambdaExpression;
                var operandBody = operand.Body;
                MemberExpression member;

                if (operandBody is UnaryExpression)
                    member = (operandBody as UnaryExpression).Operand as MemberExpression;
                else
                    member = operandBody as MemberExpression;                

                if (extendedProperty != null)
                    return new ODataCompoundQueryOperation
                    {
                        AnonymousGetManyOperation = new ODataSelectManyQueryOperation { OfType = where.Expression.OperandType(), Keys = values, NavigationProperty = member.Member.Name },
                        NavigationProperty = extendedProperty,
                        FilterExpression = filterExpression,
                        IsCountRequest = itemCount != null,
                        OrderStack = orderStack,
                        TopCount = topCount,
                        SkipCount = skipCount,
                        ProjectionExpression = projectionExpression,
                        ProjectedType = projectionType,
                        ProjectedProperties = projectedProperties,
                        Parent = EvalForQueue(tuples) as ODataSelectOneQueryOperation
                    };

                return new ODataSelectManyQueryOperation
                {
                    OfType = where.Expression.OperandType(),
                    Keys = values,
                    NavigationProperty = member.Member.Name,
                    FilterExpression = filterExpression,
                    OrderStack = orderStack,
                    TopCount = topCount,
                    SkipCount = skipCount,
                    ProjectionExpression = projectionExpression,
                    ProjectedType = projectionType,
                    ProjectedProperties = projectedProperties,
                    IsCountRequest = itemCount != null,
                    Parent = EvalForQueue(tuples) as ODataSelectOneQueryOperation
                };
            }

            if (item.Name == "Root")
            {
                var query = ((ConstantExpression)item.Expression).Value as IQueryable;

                if (query != null)
                {
                    return new ODataQueryOperation
                    {
                        OfType = query.ElementType,
                        FilterExpression = filterExpression,
                        OrderStack = orderStack,
                        TopCount = topCount,
                        SkipCount = skipCount,
                        IsCountRequest = itemCount != null,
                        ProjectionExpression = projectionExpression,
                        ProjectedType = projectionType,
                        ProjectedProperties = projectedProperties,
                    };
                }
            }

            return null;
        }

        private static ODataSelectOneQueryOperation CreateGetOneQuery(Queue<OperationTuple> operations, OperationTuple item, LambdaExpression projectionExpression, Type projectionType, IDictionary<string, PropertyDetails> projectedProperties)
        {
            var whereClause = item.Expression as UnaryExpression;
            var whereOperand = whereClause.Operand as LambdaExpression;
            var wherePredicate = whereOperand.Body as BinaryExpression;
            var value = (wherePredicate.Right as ConstantExpression).Value.ToString();
            var operandType = whereOperand.Type.GetGenericArguments().ElementAt(0);

            return new ODataSelectOneQueryOperation
            {
                OfType = operandType,
                Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "key", value } },
                ProjectionExpression = projectionExpression,
                ProjectedType = projectionType,
                ProjectedProperties = projectedProperties,
            };
        }

        /// <summary>
        /// Provides a type to describe an operation tuple composed by a name and a <see cref="Expression"/>.
        /// </summary>
        private class OperationTuple
        {
            /// <summary>
            /// Gets or sets the operation tuple name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the operation tuple <see cref="Expression"/>.
            /// </summary>
            public Expression Expression { get; set; }
        }
    }
}