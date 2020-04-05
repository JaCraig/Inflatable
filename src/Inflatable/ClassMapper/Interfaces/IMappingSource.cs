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
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.Utils;
using Serilog;
using System;
using System.Collections.Generic;

namespace Inflatable.ClassMapper
{
    /// <summary>
    /// Mapping source interface
    /// </summary>
    public interface IMappingSource
    {
        /// <summary>
        /// Gets a value indicating whether [apply analysis].
        /// </summary>
        /// <value><c>true</c> if [apply analysis]; otherwise, <c>false</c>.</value>
        bool ApplyAnalysis { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can read.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can write.
        /// </summary>
        /// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
        bool CanWrite { get; }

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        ListMapping<Type, Type> ChildTypes { get; }

        /// <summary>
        /// Gets the concrete types.
        /// </summary>
        /// <value>The concrete types.</value>
        Type[] ConcreteTypes { get; }

        /// <summary>
        /// Gets a value indicating whether [generate analysis].
        /// </summary>
        /// <value><c>true</c> if [generate analysis]; otherwise, <c>false</c>.</value>
        bool GenerateAnalysis { get; }

        /// <summary>
        /// Gets a value indicating whether [generate schema].
        /// </summary>
        /// <value><c>true</c> if [generate schema]; otherwise, <c>false</c>.</value>
        bool GenerateSchema { get; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        ILogger Logger { get; }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        Dictionary<Type, IMapping> Mappings { get; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        int Order { get; }

        /// <summary>
        /// Gets the parent types.
        /// </summary>
        /// <value>The parent types.</value>
        ListMapping<Type, Type> ParentTypes { get; }

        /// <summary>
        /// Gets the query provider.
        /// </summary>
        /// <value>The query provider.</value>
        QueryProviderManager QueryProvider { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        IDatabase Source { get; }

        /// <summary>
        /// Gets the type graphs.
        /// </summary>
        /// <value>The type graphs.</value>
        Dictionary<Type, Tree<Type>?> TypeGraphs { get; }

        /// <summary>
        /// Gets a value indicating whether [update schema].
        /// </summary>
        /// <value><c>true</c> if [update schema]; otherwise, <c>false</c>.</value>
        bool UpdateSchema { get; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        bool Equals(object obj);

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns></returns>
        IEnumerable<IMapping> GetChildMappings(Type objectType);

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns></returns>
        IEnumerable<IMapping> GetChildMappings<TObject>();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        int GetHashCode();

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns></returns>
        IEnumerable<IMapping> GetParentMapping(Type objectType);

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns></returns>
        IEnumerable<IMapping> GetParentMapping<TObject>();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        string ToString();
    }
}