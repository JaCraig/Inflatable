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
using SQLHelperDB;
using System.Data.Common;

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Query provider interface
    /// </summary>
    public interface IQueryProvider
    {
        /// <summary>
        /// Provider name associated with the query provider
        /// </summary>
        DbProviderFactory Provider { get; }

        /// <summary>
        /// Creates a batch for running commands
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="aopManager">The aop manager.</param>
        /// <returns>A batch object</returns>
        SQLHelper Batch(IDatabase source, Aspectus.Aspectus aopManager);

        /// <summary>
        /// Creates a generator object
        /// </summary>
        /// <typeparam name="TMappedClass">Class type to create the generator for</typeparam>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <returns>Generator object</returns>
        IGenerator<TMappedClass> CreateGenerator<TMappedClass>(IMappingSource mappingInformation)
            where TMappedClass : class;
    }
}