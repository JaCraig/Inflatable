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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.QueryProvider.Providers.SQLServer.Generators;

namespace Inflatable.QueryProvider.Providers.SQLServer
{
    /// <summary>
    /// SQL Server query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="Inflatable.QueryProvider.Interfaces.IGenerator{TMappedClass}"/>
    public class SQLServerGenerator<TMappedClass> : GeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLServerGenerator{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <exception cref="System.ArgumentNullException">mappingInformation</exception>
        public SQLServerGenerator(MappingSource mappingInformation)
            : base(mappingInformation, new IQueryGenerator[] {
                new DeleteQuery<TMappedClass>(mappingInformation),
                new InsertQuery<TMappedClass>(mappingInformation),
                new UpdateQuery<TMappedClass>(mappingInformation),
                new SelectAllQuery<TMappedClass>(mappingInformation),
                new SelectAnyQuery<TMappedClass>(mappingInformation),
                new InsertBulkQuery<TMappedClass>(mappingInformation)
            }, new LinqQueryGenerator<TMappedClass>(mappingInformation))
        {
        }
    }
}