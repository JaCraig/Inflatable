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

using System.Linq.Expressions;

namespace Inflatable.Utils
{
    /// <summary>
    /// Visitor expression visitor
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class Visitor : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Visitor"/> class.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public Visitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        /// The parameter
        /// </summary>
        private readonly ParameterExpression _parameter;

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ParameterExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node) => _parameter;
    }
}