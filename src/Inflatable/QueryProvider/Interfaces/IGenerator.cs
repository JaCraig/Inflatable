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

using Inflatable.LinqExpression;
using Inflatable.QueryProvider.Enums;
using System;
using System.Collections.Generic;

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Generator interface
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Gets the type of the associated.
        /// </summary>
        /// <value>The type of the associated.</value>
        Type AssociatedType { get; }
    }

    /// <summary>
    /// Generator interface
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    public interface IGenerator<TMappedClass> : IGenerator
        where TMappedClass : class
    {
        /// <summary>
        /// Gets the query generators.
        /// </summary>
        /// <value>The query generators.</value>
        IDictionary<QueryType, IQueryGenerator<TMappedClass>> QueryGenerators { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The resulting declarations.</returns>
        IQuery GenerateDeclarations(QueryType type);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        IQuery GenerateQuery(QueryData<TMappedClass> data);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The resulting queries.</returns>
        IQuery GenerateQuery(QueryType type, TMappedClass queryObject);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The resulting query</returns>
        IQuery GenerateQuery(QueryType type, TMappedClass queryObject, string propertyName);
    }
}