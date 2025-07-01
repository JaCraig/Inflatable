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

namespace Inflatable.LinqExpression.HelperClasses
{
    /// <summary>
    /// Subtree evaluator
    /// </summary>
    /// <seealso cref="ExpressionVisitor"/>
    public class SubtreeEvaluator : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubtreeEvaluator"/> class.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        public SubtreeEvaluator(HashSet<Expression> candidates)
        {
            Candidates = candidates ?? [];
        }

        /// <summary>
        /// Gets or sets the candidates.
        /// </summary>
        /// <value>The candidates.</value>
        private HashSet<Expression> Candidates { get; }

        /// <summary>
        /// Evals the specified exp.
        /// </summary>
        /// <param name="expression">The exp.</param>
        /// <returns>The resulting expression.</returns>
        public Expression Eval(Expression expression) => Visit(expression);

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the
        /// original expression.
        /// </returns>
        public override Expression Visit(Expression? node)
        {
            if (node is null)
            {
                return null!;
            }

            return Candidates.Contains(node) ? Evaluate(node) : base.Visit(node);
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The result</returns>
        private static Expression Evaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant || expression.NodeType == ExpressionType.New)
            {
                return expression;
            }

            var Result = Expression.Lambda(expression)
                                    .Compile()
                                    .DynamicInvoke(null);
            return Expression.Constant(Result, expression.Type);
        }
    }
}