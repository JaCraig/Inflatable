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
using System.Linq.Expressions;

namespace Inflatable.LinqExpression.HelperClasses
{
    /// <summary>
    /// Nominator expression visitor
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class Nominator : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nominator"/> class.
        /// </summary>
        /// <param name="functionCanBeEvaluated">The function can be evaluated.</param>
        public Nominator(Func<Expression, bool> functionCanBeEvaluated)
        {
            FunctionCanBeEvaluated = functionCanBeEvaluated;
            Candidates = new HashSet<Expression>();
        }

        /// <summary>
        /// Gets or sets the candidates.
        /// </summary>
        /// <value>The candidates.</value>
        private HashSet<Expression> Candidates { get; set; }

        /// <summary>
        /// The can not be evaluated
        /// </summary>
        /// <value><c>true</c> if this instance can not be evaluated; otherwise, <c>false</c>.</value>
        private bool CanNotBeEvaluated { get; set; }

        /// <summary>
        /// The function can be evaluated
        /// </summary>
        /// <value>The function can be evaluated.</value>
        private Func<Expression, bool> FunctionCanBeEvaluated { get; }

        /// <summary>
        /// Nominates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result</returns>
        public HashSet<Expression> Nominate(Expression expression)
        {
            Candidates = new HashSet<Expression>();
            Visit(expression);
            return Candidates;
        }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        public override Expression Visit(Expression node)
        {
            if (node is null)
                return node!;
            var saveCannotBeEvaluated = CanNotBeEvaluated;
            CanNotBeEvaluated = false;
            base.Visit(node);
            if (!CanNotBeEvaluated)
            {
                if (FunctionCanBeEvaluated(node))
                {
                    Candidates.Add(node);
                }
                else
                {
                    CanNotBeEvaluated = true;
                }
            }
            CanNotBeEvaluated |= saveCannotBeEvaluated;
            return node;
        }
    }
}