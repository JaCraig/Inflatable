/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Inflatable.LinqExpression
{
    /// <summary>
    /// Query translator
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class QueryTranslator : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator"/> class.
        /// </summary>
        public QueryTranslator()
        {
            Builder = new StringBuilder();
        }

        /// <summary>
        /// The binary operators
        /// </summary>
        private IDictionary<ExpressionType, string> BinaryOperators = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.And] = " AND ",
            [ExpressionType.Or] = " OR ",
            [ExpressionType.Equal] = " = ",
            [ExpressionType.NotEqual] = " <> ",
            [ExpressionType.LessThan] = " < ",
            [ExpressionType.LessThanOrEqual] = " <= ",
            [ExpressionType.GreaterThan] = " > ",
            [ExpressionType.GreaterThanOrEqual] = " >= "
        };

        private IDictionary<TypeCode, Func<object, string>> ConstantConverters = new Dictionary<TypeCode, Func<object, string>>
        {
            [TypeCode.Boolean] = x => (bool)x ? "1" : "0",
            [TypeCode.String] = x => "'" + x + "'",
            [TypeCode.Object] = x => throw new NotSupportedException($"The constant for ‘{x}’ is not supported"))
        };

        /// <summary>
        /// The unary operators
        /// </summary>
        private IDictionary<ExpressionType, string> UnaryOperators = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Not] = " NOT "
        };

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        private StringBuilder Builder { get; set; }

        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The resulting query string.</returns>
        public string Translate(Expression expression)
        {
            Builder = new StringBuilder();
            Visit(expression);
            return Builder.ToString();
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (!BinaryOperators.ContainsKey(node.NodeType))
                throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported.");
            Builder.Append(" (");
            Visit(node.Left);
            Builder.Append(BinaryOperators[node.NodeType]);
            Visit(node.Right);
            return node;
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The expression</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            IQueryable TempQuery = node.Value as IQueryable;
            if (TempQuery != null)
            {
                Builder.Append("SELECT * FROM ");
                Builder.Append(TempQuery.ElementType.Name);
                return node;
            }
            if (node.Value == null)
            {
                Builder.Append("NULL");
                return node;
            }
            var TypeCode = Type.GetTypeCode(node.Value.GetType());
            Builder.Append(ConstantConverters.ContainsKey(TypeCode) ? ConstantConverters[TypeCode](node.Value) : node.Value);
            return node;
        }

        /// <summary>
        /// Visits the member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The node</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                Builder.Append(node.Member.Name);
                return node;
            }
            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported.");
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Where")
            {
                Builder.Append("SELECT * FROM (");
                Visit(node.Arguments[0]);
                Builder.Append(") AS T WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                Visit(lambda.Body);
                return node;
            }
            throw new NotSupportedException($"The method '{node.Method.Name}' is not supported.");
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (!UnaryOperators.ContainsKey(node.NodeType))
                throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported.");
            Builder.Append(UnaryOperators[node.NodeType]);
            Visit(node.Operand);
            return node;
        }

        /// <summary>
        /// Strips the quotes from the query.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The query without the quotes.</returns>
        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }
    }
}