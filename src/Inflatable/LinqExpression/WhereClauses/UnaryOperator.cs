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
using System.Linq.Expressions;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Unary operator class
    /// </summary>
    /// <seealso cref="IOperator"/>
    public class UnaryOperator : IOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperator"/> class.
        /// </summary>
        /// <param name="internalOperator">The iternal operator.</param>
        /// <param name="operatorType">Type of the operator.</param>
        /// <param name="nodeType">Type of the node.</param>
        /// <exception cref="ArgumentNullException">iternalOperator</exception>
        public UnaryOperator(IOperator internalOperator, ExpressionType operatorType, Type nodeType)
        {
            InternalOperator = internalOperator ?? throw new ArgumentNullException(nameof(internalOperator));
            Operator = operatorType;
            InternalOperator.Parent = this;
            TypeCode = InternalOperator.TypeCode;
            if (Operator == ExpressionType.Convert)
            {
                TypeCode = nodeType;
            }
        }

        /// <summary>
        /// Gets the iternal operator.
        /// </summary>
        /// <value>The iternal operator.</value>
        public IOperator InternalOperator { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
        public bool IsNull { get; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <value>The operator.</value>
        public ExpressionType Operator { get; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IOperator? Parent { get; set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        public Type TypeCode { get; }

        /// <summary>
        /// The converter
        /// </summary>
        private static readonly IDictionary<ExpressionType, Func<UnaryOperator, string>> Converter = new Dictionary<ExpressionType, Func<UnaryOperator, string>>
        {
            [ExpressionType.Not] = x => $"NOT {x.InternalOperator}",
            [ExpressionType.Convert] = x => x.InternalOperator.ToString()
        };

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy() => new UnaryOperator(InternalOperator.Copy(), Operator, TypeCode);

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters() => InternalOperator.GetParameters();

        /// <summary>
        /// Does a logical negation of the operator.
        /// </summary>
        /// <returns>The resulting operator.</returns>
        public IOperator LogicallyNegate()
        {
            if (Operator == ExpressionType.Not)
            {
                return InternalOperator;
            }

            InternalOperator = InternalOperator.LogicallyNegate();
            return this;
        }

        /// <summary>
        /// Optimizes the operator based on the mapping source.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <returns></returns>
        public IOperator Optimize(IMappingSource mappingSource)
        {
            InternalOperator = InternalOperator.Optimize(mappingSource);
            if (InternalOperator is null)
            {
                return null!;
            }

            return Operator == ExpressionType.Not ? InternalOperator.LogicallyNegate() : (this);
        }

        /// <summary>
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        public void SetColumnNames(IMappingSource mappingSource, IMapping mapping) => InternalOperator?.SetColumnNames(mappingSource, mapping);

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => "(" + Converter[Operator](this) + ")";
    }
}