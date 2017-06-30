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
using Inflatable.QueryProvider.Interfaces;
using System.Data;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// SQL Server Linq query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="Interfaces.ILinqQueryGenerator{TMappedClass}"/>
    public class LinqQueryGenerator<TMappedClass> : ILinqQueryGenerator<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public IQuery GenerateQuery(QueryData<TMappedClass> data)
        {
            return new Query(CommandType.Text, "", QueryType.All);
        }
    }
}