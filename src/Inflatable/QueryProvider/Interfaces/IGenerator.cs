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

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Generator interface
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    public interface IGenerator<TMappedClass> : IGenerator
        where TMappedClass : class
    {
    }

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

        /// <summary>
        /// Generates the default queries associated with the mapped type.
        /// </summary>
        /// <returns>The default queries for the specified type.</returns>
        Queries GenerateDefaultQueries();
    }
}