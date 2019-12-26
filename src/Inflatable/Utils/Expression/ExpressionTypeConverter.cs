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

namespace Inflatable.Utils
{
    /// <summary>
    /// Expression type converter
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TReturn">The type of the return.</typeparam>
    public class ExpressionTypeConverter<TData, TReturn>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTypeConverter{TData, TReturn}"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ExpressionTypeConverter(Expression<Func<TData, TReturn>> expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TData, TReturn>> Expression { get; set; }

        /// <summary>
        /// Converts this instance.
        /// </summary>
        /// <typeparam name="TNewType">The type of the new type.</typeparam>
        /// <returns></returns>
        public Expression<Func<TNewType, TReturn>> Convert<TNewType>()
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TNewType));
            var body = new Visitor(param).Visit(Expression.Body);
            return System.Linq.Expressions.Expression.Lambda<Func<TNewType, TReturn>>(body, param);
        }
    }
}