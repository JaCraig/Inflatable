﻿/*
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
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Property operator
    /// </summary>
    /// <seealso cref="IOperator"/>
    public class Property : IOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="ArgumentNullException">property</exception>
        public Property(PropertyInfo property, int count)
        {
            Count = count;
            InternalProperty = property ?? throw new ArgumentNullException(nameof(property));
            TypeCode = InternalProperty.PropertyType;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>The property.</value>
        public PropertyInfo InternalProperty { get; private set; }

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
        /// Gets or sets the column.
        /// </summary>
        /// <value>The column.</value>
        private string Column { get; set; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy()
        {
            return new Property(InternalProperty, Count);
        }

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters()
        {
            return new List<IParameter>();
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
            var Mapping = mappingSource.Mappings[InternalProperty.DeclaringType];
            var ParentMappings = mappingSource.GetParentMapping(InternalProperty.DeclaringType);
            if (!Mapping.ContainsProperty(InternalProperty.Name))
            {
                var ParentMapping = ParentMappings.FirstOrDefault(x => x.ContainsProperty(InternalProperty.Name));
                if (ParentMapping == null)
                    return null;
                Column = ParentMapping.GetColumnName(InternalProperty.Name);
            }
            else
                Column = Mapping.GetColumnName(InternalProperty.Name);

            if (InternalProperty.PropertyType == typeof(bool) && Parent as BinaryOperator == null)
            {
                return new BinaryOperator(this, new Constant(true, Count), ExpressionType.Equal);
            }
            return this;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Column) ? InternalProperty.Name : Column;
        }
    }
}