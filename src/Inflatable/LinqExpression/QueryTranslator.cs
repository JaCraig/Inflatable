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
using Inflatable.LinqExpression.OrderBy;
using Inflatable.LinqExpression.OrderBy.Enums;
using Inflatable.LinqExpression.Select;
using Inflatable.LinqExpression.WhereClauses;
using Inflatable.QueryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.LinqExpression
{
    /// <summary>
    /// Query translator
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor"/>
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
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead && x.GetChildMappings(typeof(TObject)).Any()))
            {
                Builders.Add(Source, new QueryData<TObject>(Source));
            }
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
        private Dictionary<MappingSource, QueryData<TObject>> Builders { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        private int Count { get; set; }

        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The resulting query string.</returns>
        public IDictionary<MappingSource, QueryData<TObject>> Translate(Expression expression)
        {
            Visit(expression);
            foreach (var Key in Builders.Keys)
            {
                Builders[Key].WhereClause.Optimize(Key);
                foreach (var Parameter in Builders[Key].WhereClause.GetParameters())
                {
                    Builders[Key].Parameters.Add(Parameter);
                }
            }
            return Builders;
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
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.Name == "Where")
                {
                    node = (MethodCallExpression)Evaluator.PartialEval(node);
                    Visit(node.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    var CurrentClause = new WhereVisitor<TObject>(Count).WhereProjection(lambda.Body);
                    ++Count;
                    foreach (var Source in Builders.Keys)
                    {
                        Builders[Source].WhereClause.Combine(CurrentClause.Copy());
                    }
                    return node;
                }
                if (node.Method.Name == "Select")
                {
                    Visit(node.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    var Columns = new ColumnProjector().ProjectColumns(lambda.Body);
                    foreach (var Source in Builders.Keys)
                    {
                        var ParentMappings = Source.GetChildMappings(typeof(TObject))
                                                   .SelectMany(x => Source.GetParentMapping(x.ObjectType))
                                                   .Distinct();
                        foreach (var Column in Columns)
                        {
                            if (ParentMappings.Any(x => x.ContainsProperty(Column.Name)))
                            {
                                Builders[Source].SelectValues.Add(Column);
                            }
                        }
                    }
                    return node;
                }
                if (node.Method.Name == "ThenBy"
                    || node.Method.Name == "OrderBy"
                    || node.Method.Name == "OrderByDescending"
                    || node.Method.Name == "ThenByDescending")
                {
                    Visit(node.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    var Columns = new ColumnProjector().ProjectColumns(lambda.Body);
                    foreach (var Source in Builders.Keys)
                    {
                        var ParentMappings = Source.GetChildMappings(typeof(TObject))
                                                   .SelectMany(x => Source.GetParentMapping(x.ObjectType))
                                                   .Distinct();
                        foreach (var Column in Columns)
                        {
                            if (ParentMappings.Any(x => x.ContainsProperty(Column.Name)))
                            {
                                Builders[Source].OrderByValues.Add(new OrderByClause(Builders[Source].OrderByValues.Count,
                                    Column,
                                    node.Method.Name.Contains("Descending") ? Direction.Descending : Direction.Ascending));
                            }
                        }
                    }
                    return node;
                }
                if (node.Method.Name == "Distinct")
                {
                    Visit(node.Arguments[0]);
                    foreach (var Source in Builders.Keys)
                    {
                        Builders[Source].Distinct = true;
                    }
                    return node;
                }
                if (node.Method.Name == "First"
                    || node.Method.Name == "FirstOrDefault"
                    || node.Method.Name == "Single"
                    || node.Method.Name == "SingleOrDefault")
                {
                    Visit(node.Arguments[0]);
                    foreach (var Source in Builders.Keys)
                    {
                        Builders[Source].Top = 1;
                    }
                    return node;
                }
                if (node.Method.Name == "Take")
                {
                    Visit(node.Arguments[0]);
                    foreach (var Source in Builders.Keys)
                    {
                        Builders[Source].Top = (int)(node.Arguments[1] as ConstantExpression)?.Value;
                    }
                    return node;
                }
            }
            throw new NotSupportedException($"The method '{node.Method.Name}' is not supported.");
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