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

using Inflatable.LinqExpression.WhereClauses.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Where visitor
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="ExpressionVisitor"/>
    public class WhereVisitor<TObject> : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="count">The count.</param>
        public WhereVisitor(int count)
        {
            Count = count;
            CurrentClause = new EmptyClause();
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the current clause.
        /// </summary>
        /// <value>The current clause.</value>
        private IOperator CurrentClause { get; set; }

        /// <summary>
        /// Wheres the projection.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The current clause</returns>
        public IOperator WhereProjection(Expression expression)
        {
            Visit(expression);
            return CurrentClause;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        /// <exception cref="NotSupportedException"></exception>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node is null)
                return node!;
            Visit(node.Left);
            var LeftSide = CurrentClause;
            Visit(node.Right);
            CurrentClause = new BinaryOperator(LeftSide, CurrentClause, node.NodeType);
            return node;
        }

        /// <summary>
        /// Visits the constant.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The expression</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node is null || node.Value is IQueryable)
            {
                return node!;
            }

            CurrentClause = new Constant(node.Value!, Count);
            ++Count;
            return node;
        }

        /// <summary>
        /// Visits the member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The node</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node is null)
                return node!;
            var TempProperty = node.Member as PropertyInfo;
            if (node.Expression?.NodeType == ExpressionType.Parameter && TempProperty != null)
            {
                CurrentClause = new Property<TObject>(TempProperty, Count);
                ++Count;
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
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node is null)
                return node!;
            Visit(node.Object);
            var TempProperty = CurrentClause;
            Visit(node.Arguments);
            var TempValue = CurrentClause;
            if (node.Method.Name == "StartsWith")
                CurrentClause = new LikeOperator(TempProperty, TempValue, node.Method.Name);
            else if (node.Method.Name == "EndsWith")
                CurrentClause = new LikeOperator(TempProperty, TempValue, node.Method.Name);
            else if (node.Method.Name == "Contains")
                CurrentClause = new LikeOperator(TempProperty, TempValue, node.Method.Name);
            else
                throw new NotSupportedException($"The method '{node.Method.Name}' is not supported.");
            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        /// <exception cref="NotSupportedException"></exception>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node is null)
                return node!;
            Visit(node.Operand);
            CurrentClause = new UnaryOperator(CurrentClause, node.NodeType, node.Type);
            return node;
        }
    }
}