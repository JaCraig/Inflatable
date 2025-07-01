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
using Inflatable.Interfaces;
using Inflatable.LinqExpression.WhereClauses.Interfaces;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Like operator
    /// </summary>
    /// <seealso cref="IOperator"/>
    public class LikeOperator : IOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LikeOperator"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="methodType">Type of the method.</param>
        /// <exception cref="ArgumentNullException">value or property</exception>
        public LikeOperator(IOperator? property, IOperator? value, string methodType)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            TypeCode = typeof(bool);
            Property.Parent = this;
            Value.Parent = this;
            MethodType = methodType;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
        public bool IsNull { get; }

        /// <summary>
        /// Gets the type of the method.
        /// </summary>
        /// <value>The type of the method.</value>
        public string MethodType { get; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IOperator? Parent { get; set; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>The property.</value>
        public IOperator? Property { get; private set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        public Type TypeCode { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public IOperator? Value { get; private set; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy() => new LikeOperator(Property?.Copy(), Value?.Copy(), MethodType);

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters()
        {
            var ReturnValue = new List<IParameter>();
            ReturnValue.AddRange(Property?.GetParameters() ?? []);
            ReturnValue.AddRange(Value?.GetParameters() ?? []);
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
        /// <returns></returns>
        public IOperator? Optimize(IMappingSource mappingSource)
        {
            Property = Property?.Optimize(mappingSource);
            if (Property is null)
                return null;
            Value = Value?.Optimize(mappingSource);
            if (Value is Constant TempConstant)
            {
                var Val = TempConstant.Value?.ToString();
                if (!string.IsNullOrEmpty(Val))
                {
                    if (MethodType == "StartsWith")
                        TempConstant.Value = Val + "%";
                    else if (MethodType == "EndsWith")
                        TempConstant.Value = "%" + Val;
                    else if (MethodType == "Contains")
                        TempConstant.Value = "%" + Val + "%";
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        public void SetColumnNames(IMappingSource mappingSource, IMapping mapping)
        {
            Property?.SetColumnNames(mappingSource, mapping);
            Value?.SetColumnNames(mappingSource, mapping);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "(" + Property + " LIKE " + Value + ")";
        }
    }
}