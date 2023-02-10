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
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;

namespace Inflatable.LinqExpression.WhereClauses.Interfaces
{
    /// <summary>
    /// Operator interface
    /// </summary>
    public interface IOperator
    {
        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
        bool IsNull { get; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        IOperator? Parent { get; set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        Type TypeCode { get; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        IOperator Copy();

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        List<IParameter> GetParameters();

        /// <summary>
        /// Does a logical negation of the operator.
        /// </summary>
        /// <returns>The resulting operator.</returns>
        IOperator LogicallyNegate();

        /// <summary>
        /// Optimizes the operator based on the mapping source.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        IOperator? Optimize(IMappingSource mappingSource);

        /// <summary>
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        void SetColumnNames(IMappingSource mappingSource, IMapping mapping);
    }
}