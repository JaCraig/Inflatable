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
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// Insert query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="QueryGeneratorBaseClass{TMappedClass}"/>
    public class InsertQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <param name="objectPool">The object pool.</param>
        public InsertQuery(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
            : base(mappingInformation, objectPool)
        {
            if (!MappingInformation.TypeGraphs.TryGetValue(AssociatedType, out Utils.Tree<Type>? TypeGraph))
            {
                return;
            }

            QueryDeclarationText = GenerateInsertQueryDeclarations();
            QueryText = GenerateInsertQuery(TypeGraph?.Root);
            var ParentMappings = MappingInformation.GetParentMapping(typeof(TMappedClass));

            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            ReferenceProperties = ParentMappings.SelectMany(x => x.ReferenceProperties);
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Insert;

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty>? IDProperties { get; }

        /// <summary>
        /// Gets or sets the query declaration text.
        /// </summary>
        /// <value>The query declaration text.</value>
        private string[]? QueryDeclarationText { get; }

        /// <summary>
        /// Gets or sets the query text.
        /// </summary>
        /// <value>The query text.</value>
        private string? QueryText { get; }

        /// <summary>
        /// Gets or sets the reference properties.
        /// </summary>
        /// <value>The reference properties.</value>
        private IEnumerable<IProperty>? ReferenceProperties { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations()
        {
            if (QueryDeclarationText is null)
                return [];
            var ReturnValue = new List<IQuery>();
            for (var X = 0; X < QueryDeclarationText.Length; ++X)
            {
                ReturnValue.Add(new Query(AssociatedType, CommandType.Text, QueryDeclarationText[X], QueryType));
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Generates the insert query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject) => [new Query(AssociatedType, CommandType.Text, QueryText ?? "", QueryType, GenerateParameters(queryObject))];

        /// <summary>
        /// Generates the insert query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The resulting query</returns>
        private string GenerateInsertQuery(Utils.TreeNode<Type>? node)
        {
            if (node is null)
                return "";
            var Builder = ObjectPool.Get();
            var ParameterList = ObjectPool.Get();
            var ValueList = ObjectPool.Get();
            var DeclareProperties = ObjectPool.Get();
            var SetProperties = ObjectPool.Get();
            var IDReturn = ObjectPool.Get();
            var Splitter = "";
            var Mapping = MappingInformation.Mappings[node.Data];

            //Generate parent queries
            for (int X = 0, NodeNodesCount = node.Nodes.Count; X < NodeNodesCount; X++)
            {
                var Parent = node.Nodes[X];
                Builder.AppendLine(GenerateInsertQuery(Parent));
            }

            //Reference properties
            foreach (var ReferenceProperty in Mapping.ReferenceProperties
                                                     .Where(x => string.IsNullOrEmpty(x.ComputedColumnSpecification)))
            {
                ParameterList.Append(Splitter).Append(GetColumnName(ReferenceProperty));
                ValueList.Append(Splitter).Append(GetParameterName(ReferenceProperty));
                Splitter = ",";
            }

            //Non auto incremented ID Properties
            foreach (var IDProperty in Mapping.IDProperties.Where(x => !x.AutoIncrement))
            {
                ParameterList.Append(Splitter).Append(GetColumnName(IDProperty));
                ValueList.Append(Splitter).Append(GetParameterName(IDProperty));
                Splitter = ",";
            }

            //Parent ID and auto ID properties
            foreach (var ParentMapping in node.Nodes.ForEach(x => MappingInformation.Mappings[x.Data]))
            {
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    ParameterList.Append(Splitter).Append(GetParentColumnName(Mapping, IDProperty));
                    ValueList.Append(Splitter).Append(GetParentParameterName(IDProperty));
                    Splitter = ",";
                }
                foreach (var AutoIDProperty in ParentMapping.AutoIDProperties)
                {
                    ParameterList.Append(Splitter).Append(GetParentColumnName(Mapping, AutoIDProperty));
                    ValueList.Append(Splitter).Append(GetParentParameterName(AutoIDProperty));
                    Splitter = ",";
                }
            }

            //ID Properties to pass to the next set of queries
            foreach (var IDProperty in Mapping.IDProperties)
            {
                SetProperties.Append("SET ").Append(GetParentParameterName(IDProperty)).Append('=').Append(IDProperty.AutoIncrement ? "SCOPE_IDENTITY()" : GetParameterName(IDProperty)).AppendLine(";");
                if (IDProperty.AutoIncrement)
                {
                    IDReturn.AppendLineFormat("SELECT {0} AS [{1}];", GetParentParameterName(IDProperty), IDProperty.Name);
                }
            }

            //Auto ID properties to pass to the next set of queries
            foreach (var AutoIDProperty in Mapping.AutoIDProperties)
            {
                SetProperties.Append("SET ").Append(GetParentParameterName(AutoIDProperty)).AppendLine("=SCOPE_IDENTITY();");
            }

            //Build the actual queries
            if (node.Parent != null || Mapping.IDProperties.Any(x => x.AutoIncrement))
            {
                Builder.Append(DeclareProperties);
            }
            if (Mapping.IDProperties.All(x => x.AutoIncrement) && Mapping.ReferenceProperties.Count == 0 && Mapping.AutoIDProperties.Count == 0)
            {
                Builder.AppendLineFormat("INSERT INTO {0} DEFAULT VALUES;", GetTableName(Mapping));
            }
            else
            {
                Builder.AppendLineFormat("INSERT INTO {0}({1}) VALUES ({2});", GetTableName(Mapping), ParameterList, ValueList);
            }
            if (node.Parent != null || Mapping.IDProperties.Any(x => x.AutoIncrement))
            {
                Builder.Append(SetProperties);
                if (Mapping.IDProperties.Any(x => x.AutoIncrement))
                {
                    Builder.Append(IDReturn);
                }
            }
            var Result = Builder.ToString();
            ObjectPool.Return(Builder);
            ObjectPool.Return(ParameterList);
            ObjectPool.Return(ValueList);
            ObjectPool.Return(DeclareProperties);
            ObjectPool.Return(SetProperties);
            ObjectPool.Return(IDReturn);
            return Result;
        }

        /// <summary>
        /// Generates the insert query declarations.
        /// </summary>
        /// <returns>The resulting query</returns>
        private string[] GenerateInsertQueryDeclarations()
        {
            var Builder = new List<string>();
            var ParentMappings = new List<IMapping>();
            foreach (var ChildMapping in MappingInformation.GetChildMappings(typeof(TMappedClass)))
            {
                var TempParentMappings = MappingInformation.GetParentMapping(ChildMapping.ObjectType);
                ParentMappings.AddIfUnique(TempParentMappings);
            }
            for (int X = 0, ParentMappingsCount = ParentMappings.Count; X < ParentMappingsCount; X++)
            {
                var ParentMapping = ParentMappings[X];
                //ID Properties to pass to the next set of queries
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    Builder.Add("DECLARE " + GetParentParameterName(IDProperty) + " AS " + GetParameterType(IDProperty) + ";");
                }

                //Auto ID properties to pass to the next set of queries
                foreach (var AutoIDProperty in ParentMapping.AutoIDProperties)
                {
                    Builder.Add("DECLARE " + GetParentParameterName(AutoIDProperty) + " AS " + GetParameterType() + ";");
                }
            }

            return [.. Builder];
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters</returns>
        private IParameter?[] GenerateParameters(TMappedClass queryObject)
        {
            var Parameters = IDProperties?.ForEach(y => y.GetColumnInfo()[0].GetAsParameter(queryObject)).ToList();
            Parameters?.AddRange(ReferenceProperties?.ForEach(y => y.GetColumnInfo()[0].GetAsParameter(queryObject)) ?? []);
            return Parameters?.ToArray() ?? [];
        }
    }
}