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

using Inflatable.ClassMapper;
using Inflatable.LinqExpression.HelperClasses;
using Inflatable.LinqExpression.WhereClauses;
using Inflatable.LinqExpression.WhereClauses.Interfaces;
using Inflatable.QueryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.LinqExpression
{
    /// <summary>
    /// Query translator
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class QueryTranslator<TObject> : ExpressionVisitor
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator{TObject}"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <exception cref="ArgumentNullException">mappingManager or queryProviderManager</exception>
        public QueryTranslator(MappingManager mappingManager,
            QueryProviderManager queryProviderManager)
        {
            MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            Builders = new Dictionary<MappingSource, QueryData<TObject>>();
        }

        /// <summary>
        /// Gets the mapping manager.
        /// </summary>
        /// <value>The mapping manager.</value>
        public MappingManager MappingManager { get; }

        /// <summary>
        /// Gets the query provider manager.
        /// </summary>
        /// <value>The query provider manager.</value>
        public QueryProviderManager QueryProviderManager { get; }

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>The builder.</value>
        private Dictionary<MappingSource, QueryData<TObject>> Builders { get; set; }

        private IOperator CurrentClause { get; set; }

        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The resulting query string.</returns>
        public IDictionary<MappingSource, QueryData<TObject>> Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            Builders = new Dictionary<MappingSource, QueryData<TObject>>();
            foreach (var Source in MappingManager.Sources)
            {
                Builders.Add(Source, new QueryData<TObject>(Source));
            }
            Visit(expression);
            foreach (var Key in Builders.Keys)
            {
                Builders[Key].WhereClause.Optimize(Key);
            }
            return Builders;
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
            if (node.Value is IQueryable)
                return node;
            CurrentClause = new Constant(node.Value);
            return node;
        }

        /// <summary>
        /// Visits the member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The node</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            var TempProperty = node.Member as PropertyInfo;
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter && TempProperty != null)
            {
                CurrentClause = new Property(TempProperty);
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
                Visit(node.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                Visit(lambda.Body);
                foreach (var Source in Builders.Keys)
                {
                    Builders[Source].WhereClause.Combine(CurrentClause.Copy());
                }
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
            Visit(node.Operand);
            CurrentClause = new UnaryOperator(CurrentClause, node.NodeType, node.Type);
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