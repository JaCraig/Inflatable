using BigBook;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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
            IDProperties = MappingInformation.GetChildMappings(typeof(TMappedClass))
                                             .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                             .Distinct()
                                             .SelectMany(x => x.IDProperties);
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType { get; } = QueryType.LoadData;

        /// <summary>
        /// Gets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; }

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
            return GenerateQueries(Array.Empty<Dynamo>());
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>The resulting query</returns>
        public IQuery[] GenerateQueries(Dynamo[] ids)
        {
            if (ids is null || ids.Length == 0)
                return Array.Empty<IQuery>();
            var ReturnValue = new List<IQuery>();
            var ItemSize = ids.FirstOrDefault()?.Count ?? 1;
            foreach (var ChildMapping in MappingInformation.GetChildMappings(typeof(TMappedClass)))
            {
                var TypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];
                foreach (var Split in SplitList(ids, 1000 / ItemSize))
                {
                    ReturnValue.Add(new Query(ChildMapping.ObjectType, CommandType.Text, GenerateSelectQuery(TypeGraph?.Root, Split), QueryType, GetParameters(Split, IDProperties)));
                }
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Splits the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="nSize">Size of the n.</param>
        /// <returns></returns>
        private static IEnumerable<Memory<T>> SplitList<T>(T[] values, int nSize = 1000)
        {
            var ValueSpan = new Memory<T>(values);
            for (int i = 0; i < values.Length; i += nSize)
            {
                yield return ValueSpan.Slice(i, Math.Min(nSize, values.Length - i));
            }
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                var IDProperties = ObjectPool.Get();
                var Separator = string.Empty;
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    IDProperties.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    IDProperties.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                Result.AppendLine()
                    .AppendFormat(CultureInfo.InvariantCulture, "INNER JOIN {0} ON {1}", GetTableName(ParentMapping), IDProperties);
                ObjectPool.Return(IDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }

            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the parameter list.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateParameterList(Utils.TreeNode<Type> node)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            var Separator = string.Empty;
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentResult = GenerateParameterList(ParentNode);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator).Append(ParentResult);
                    Separator = ",";
                }
            }

            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1} AS {2}", Separator, GetColumnName(IDProperty), "[" + IDProperty.Name + "]");
                Separator = ",";
            }
            foreach (var ReferenceProperty in Mapping.ReferenceProperties)
            {
                Result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1} AS {2}", Separator, GetColumnName(ReferenceProperty), "[" + ReferenceProperty.Name + "]");
                Separator = ",";
            }
            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.TreeNode<Type>? node, Memory<Dynamo> ids)
        {
            if (node is null)
                return string.Empty;
            var Builder = ObjectPool.Get();
            var ParameterList = ObjectPool.Get();
            var FromClause = ObjectPool.Get();
            var WhereClause = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];

            //Get From Clause
            FromClause.Append(GetTableName(Mapping))
                .Append(GenerateFromClause(node));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(node));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(ids));

            //Generate final query
            Builder
                .Append("SELECT ")
                .Append(ParameterList)
                .AppendLine()
                .Append("FROM ")
                .Append(FromClause)
                .AppendLine();
            if (WhereClause.Length > 0)
            {
                Builder.Append("WHERE ")
                       .Append(WhereClause)
                       .AppendLine();
            }
            var ReturnValue = Builder.ToString().TrimEnd('\r', '\n', ' ', '\t') + ";";
            ObjectPool.Return(Builder);
            ObjectPool.Return(ParameterList);
            ObjectPool.Return(FromClause);
            ObjectPool.Return(WhereClause);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <returns>The WHERE clause</returns>
        private string GenerateWhereClause(Memory<Dynamo> ids)
        {
            var Result = ObjectPool.Get();
            var Separator2 = string.Empty;
            for (int x = 0; x < ids.Length; ++x)
            {
                var Separator = string.Empty;
                Result.AppendFormat("{0}(", Separator2);
                foreach (var IDMapping in IDProperties)
                {
                    Result.AppendFormat("{0}{1}={2}",
                        Separator,
                        "[" + IDMapping.ParentMapping?.SchemaName + "].[" + IDMapping.ParentMapping?.TableName + "].[" + IDMapping.ColumnName + "]",
                        GetParameterName(IDMapping) + x);
                    Separator = "\r\nAND ";
                }
                Result.Append(")");
                Separator2 = "\r\nOR ";
            }
            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="idProperties">The identifier properties.</param>
        /// <returns>The parameters</returns>
        private IParameter?[] GetParameters(Memory<Dynamo> ids, IEnumerable<IIDProperty> idProperties)
        {
            List<IParameter?> ReturnValues = new List<IParameter?>();
            var IDSpan = ids.Span;
            for (int x = 0; x < IDSpan.Length; ++x)
            {
                var Value = IDSpan[x];
                var TempParams = idProperties.ForEach(x => x.GetColumnInfo()[0].GetAsParameter(Value)).ForEach(y => { y.ID += x; });
                ReturnValues.AddRange(TempParams ?? Array.Empty<IParameter?>());
            }
            return ReturnValues.ToArray();
        }
    }
}