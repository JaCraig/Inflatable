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

using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression.WhereClauses.Interfaces;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// <param name="count">The count.</param>
        public Constant(object value, int count)
        {
            Count = count;
            Value = value;
            if (value != null)
                TypeCode = Value.GetType();
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; }

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
            return new Constant(Value, Count);
        }

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters()
        {
            List<IParameter> ReturnValue = new List<IParameter>();
            if (Value == null)
            {
                ReturnValue.Add(new Parameter<object>(Count.ToString(), null));
            }
            else if (Value as string != null)
            {
                ReturnValue.Add(new StringParameter(Count.ToString(), Value as string));
            }
            else
            {
                ReturnValue.Add(new Parameter<object>(Count.ToString(), Value.GetType().To(DbType.Int32), Value));
            }
            return ReturnValue;
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
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        public void SetColumnNames(MappingSource mappingSource, IMapping mapping)
        {
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
            return "@" + Count;
        }
    }
}