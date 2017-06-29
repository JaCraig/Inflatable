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
using Inflatable.LinqExpression.WhereClauses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Constant operator
    /// </summary>
    /// <seealso cref="IOperator"/>
    public class Constant : IOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Constant"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Constant(object value)
        {
            Value = value;
            if (value != null)
                TypeCode = Value.GetType();
        }

        /// <summary>
        /// The constant converters
        /// </summary>
        private readonly IDictionary<Type, Func<object, string>> ConstantConverters = new Dictionary<Type, Func<object, string>>
        {
            [typeof(bool)] = x => (bool)x ? "1" : "0",
            [typeof(string)] = x => "'" + x + "'",
            [typeof(DateTime)] = x => "'" + x + "'",
            [typeof(char)] = x => "'" + x + "'",
            [typeof(TimeSpan)] = x => "'" + x + "'",
            [typeof(DateTimeOffset)] = x => "'" + x + "'",
            [typeof(object)] = x => throw new NotSupportedException($"The constant for ‘{x}’ is not supported")
        };

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IOperator Parent { get; set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        public Type TypeCode { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy()
        {
            return new Constant(Value);
        }

        /// <summary>
        /// Does a logical negation of the operator.
        /// </summary>
        /// <returns>The resulting operator.</returns>
        public IOperator LogicallyNegate()
        {
            return this;
        }

        /// <summary>
        /// Optimizes the operator based on the mapping source.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <returns>The result</returns>
        public IOperator Optimize(MappingSource mappingSource)
        {
            return this;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            if (Value is IQueryable TempQuery)
                return "SELECT * FROM " + TempQuery.ElementType.Name;
            if (Value == null)
                return "NULL";
            return ConstantConverters.ContainsKey(TypeCode) ? ConstantConverters[TypeCode](Value) : Value.ToString();
        }
    }
}