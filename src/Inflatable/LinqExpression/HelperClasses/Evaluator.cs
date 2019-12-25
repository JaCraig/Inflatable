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
using System.Linq.Expressions;

namespace Inflatable.LinqExpression.HelperClasses
{
    /// <summary>
    /// Evaluator static class
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Partial eval.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="fnCanBeEvaluated">The function can be evaluated.</param>
        /// <returns>The resulting expression</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated) => new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);

        /// <summary>
        /// Partial the eval.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The resulting expression</returns>
        public static Expression PartialEval(Expression expression) => PartialEval(expression, CanBeEvaluatedLocally);

        /// <summary>
        /// Determines whether this instance [can be evaluated locally] the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// <c>true</c> if this instance [can be evaluated locally] the specified expression;
        /// otherwise, <c>false</c>.
        /// </returns>
        private static bool CanBeEvaluatedLocally(Expression expression) => expression.NodeType != ExpressionType.Parameter;
    }
}