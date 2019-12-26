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
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Where operator
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="IOperator"/>
    public class WhereClause<TObject> : IOperator
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhereClause{TObject}"/> class.
        /// </summary>
        /// <param name="internalOperator">The internal operator.</param>
        public WhereClause(IOperator? internalOperator)
        {
            InternalOperator = internalOperator;
            if (InternalOperator != null)
            {
                InternalOperator.Parent = this;
                TypeCode = InternalOperator.TypeCode;
            }
            else
            {
                TypeCode = typeof(object);
            }
        }

        /// <summary>
        /// Gets the internal operator.
        /// </summary>
        /// <value>The internal operator.</value>
        public IOperator? InternalOperator { get; private set; }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType => typeof(TObject);

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IOperator? Parent { get; set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        public Type TypeCode { get; private set; }

        /// <summary>
        /// Combines the specified clauses.
        /// </summary>
        /// <param name="clause">The clause to combine with this one.</param>
        /// <returns>The resulting where clause.</returns>
        public WhereClause<TObject> Combine(IOperator clause)
        {
            if (clause is WhereClause<TObject> TempWhere)
            {
                clause = TempWhere.InternalOperator!;
            }

            InternalOperator = InternalOperator != null ?
                new BinaryOperator(InternalOperator, clause, ExpressionType.And) :
                clause;
            InternalOperator.Parent = this;
            TypeCode = InternalOperator.TypeCode;
            return this;
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy() => new WhereClause<TObject>(InternalOperator?.Copy());

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters() => InternalOperator == null ? new List<IParameter>() : InternalOperator.GetParameters();

        /// <summary>
        /// Does a logical negation of the operator.
        /// </summary>
        /// <returns>The resulting operator.</returns>
        public IOperator LogicallyNegate()
        {
            InternalOperator = InternalOperator?.LogicallyNegate();
            return this;
        }

        /// <summary>
        /// Optimizes the operator based on the mapping source.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        public IOperator Optimize(MappingSource mappingSource)
        {
            if (InternalOperator == null)
            {
                return this;
            }

            var IsValid = mappingSource.GetChildMappings(ObjectType).Any();
            InternalOperator = IsValid ?
                InternalOperator.Optimize(mappingSource) :
                null;
            if (InternalOperator?.TypeCode != typeof(bool))
            {
                InternalOperator = null;
            }

            return this;
        }

        /// <summary>
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        public void SetColumnNames(MappingSource mappingSource, IMapping mapping) => InternalOperator?.SetColumnNames(mappingSource, mapping);

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => InternalOperator == null ? "" : "WHERE " + InternalOperator;
    }
}