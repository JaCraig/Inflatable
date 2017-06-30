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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.LinqExpression.Select
{
    /// <summary>
    /// Finds properties from a select statement.
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class ColumnProjector : ExpressionVisitor
    {
        /// <summary>
        /// Gets or sets the found properties.
        /// </summary>
        /// <value>The found properties.</value>
        private List<PropertyInfo> FoundProperties { get; set; }

        /// <summary>
        /// Projects the columns to the desired properties.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The resulting properties</returns>
        public IEnumerable<PropertyInfo> ProjectColumns(Expression expression)
        {
            FoundProperties = new List<PropertyInfo>();
            Visit(expression);
            return FoundProperties;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                FoundProperties.Add(node.Member as PropertyInfo);
            }
            return base.VisitMember(node);
        }
    }
}