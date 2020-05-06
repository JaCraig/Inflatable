using BigBook;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// Data load query
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="QueryGeneratorBaseClass{TMappedClass}"/>
    public class DataLoadQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>, IDataQueryGenerator<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoadQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        /// <param name="objectPool">The object pool.</param>
        public DataLoadQuery(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
            : base(mappingInformation, objectPool)
        {
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType { get; } = QueryType.LoadData;

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations() => new IQuery[] { new Query(AssociatedType, CommandType.Text, string.Empty, QueryType) };

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject)
        {
            return GenerateQueries(new List<Dynamo>());
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The resulting query</returns>
        public IQuery[] GenerateQueries(List<Dynamo> ids)
        {
            return Array.Empty<IQuery>();
        }
    }
}