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

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Linq query generator
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    public interface ILinqQueryGenerator<TObject> : IQueryGenerator<TObject>
        where TObject : class
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        IQuery[] GenerateQueries(QueryData<TObject> data);
    }
}