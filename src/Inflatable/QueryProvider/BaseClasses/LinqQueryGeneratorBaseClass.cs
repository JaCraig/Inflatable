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
using Inflatable.LinqExpression;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System.Text;

namespace Inflatable.QueryProvider.BaseClasses
{
    /// <summary>
    /// Linq query generator base class
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="QueryGeneratorBaseClass{TObject}"/>
    /// <seealso cref="ILinqQueryGenerator{TObject}"/>
    public abstract class LinqQueryGeneratorBaseClass<TObject> : QueryGeneratorBaseClass<TObject>, ILinqQueryGenerator<TObject>
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQueryGeneratorBaseClass{TObject}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        /// <param name="objectPool">The object pool.</param>
        protected LinqQueryGeneratorBaseClass(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool) :
            base(mappingInformation, objectPool)
        {
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public abstract IQuery[] GenerateQueries(QueryData<TObject> data);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TObject queryObject) => GenerateQueries(new QueryData<TObject>(MappingInformation));
    }
}