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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// Insert bulk query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
    public class InsertBulkQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertBulkQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        public InsertBulkQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.InsertBulk;

        /// <summary>
        /// Generates the insert query.
        /// </summary>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery()
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new Query(CommandType.Text, GenerateInsertQuery(TypeGraph.Root), QueryType);
        }

        /// <summary>
        /// Generates the insert query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The resulting query</returns>
        private string GenerateInsertQuery(Utils.TreeNode<Type> node)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder ValueList = new StringBuilder();
            StringBuilder DeclareProperties = new StringBuilder();
            StringBuilder SetProperties = new StringBuilder();
            StringBuilder IDReturn = new StringBuilder();
            string Splitter = "";
            var Mapping = MappingInformation.Mappings[node.Data];

            //Generate parent queries
            foreach (var Parent in node.Nodes)
            {
                Builder.AppendLine(GenerateInsertQuery(Parent));
            }

            //Reference properties
            foreach (var ReferenceProperty in Mapping.ReferenceProperties
                                                     .Where(x => string.IsNullOrEmpty(x.ComputedColumnSpecification)))
            {
                ParameterList.Append(Splitter + GetColumnName(ReferenceProperty));
                ValueList.Append(Splitter + GetParameterName(ReferenceProperty));
                Splitter = ",";
            }

            //Non auto incremented ID Properties
            foreach (var IDProperty in Mapping.IDProperties.Where(x => !x.AutoIncrement))
            {
                ParameterList.Append(Splitter + GetColumnName(IDProperty));
                ValueList.Append(Splitter + GetParameterName(IDProperty));
                Splitter = ",";
            }

            //Parent ID and auto ID properties
            foreach (var ParentMapping in node.Nodes.ForEach(x => MappingInformation.Mappings[x.Data]))
            {
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    ParameterList.Append(Splitter + GetParentColumnName(Mapping, IDProperty));
                    ValueList.Append(Splitter + GetParentParameterName(IDProperty));
                    Splitter = ",";
                }
                foreach (var AutoIDProperty in ParentMapping.AutoIDProperties)
                {
                    ParameterList.Append(Splitter + GetParentColumnName(Mapping, AutoIDProperty));
                    ValueList.Append(Splitter + GetParentParameterName(AutoIDProperty));
                    Splitter = ",";
                }
            }

            //ID Properties to pass to the next set of queries
            foreach (var IDProperty in Mapping.IDProperties)
            {
                SetProperties.AppendLine("SET " + GetParentParameterName(IDProperty) + "=" + (IDProperty.AutoIncrement ? "SCOPE_IDENTITY()" : GetParameterName(IDProperty)) + ";");
                if (IDProperty.AutoIncrement)
                {
                    IDReturn.AppendLineFormat("SELECT {0} AS [{1}];", GetParentParameterName(IDProperty), IDProperty.Name);
                }
            }

            //Auto ID properties to pass to the next set of queries
            foreach (var AutoIDProperty in Mapping.AutoIDProperties)
            {
                SetProperties.AppendLine("SET " + GetParentParameterName(AutoIDProperty) + "=SCOPE_IDENTITY();");
            }

            //Build the actual queries
            if (node.Parent != null || Mapping.IDProperties.Any(x => x.AutoIncrement))
            {
                Builder.Append(DeclareProperties.ToString());
            }
            if (Mapping.IDProperties.All(x => x.AutoIncrement) && Mapping.ReferenceProperties.Count == 0)
            {
                Builder.AppendLineFormat("INSERT INTO {0} DEFAULT VALUES;", GetTableName(Mapping));
            }
            else
            {
                Builder.AppendLineFormat("INSERT INTO {0}({1}) VALUES ({2});", GetTableName(Mapping), ParameterList, ValueList);
            }
            if (node.Parent != null || Mapping.IDProperties.Any(x => x.AutoIncrement))
            {
                Builder.Append(SetProperties.ToString());
                if (Mapping.IDProperties.Any(x => x.AutoIncrement))
                {
                    Builder.Append(IDReturn);
                }
            }
            return Builder.ToString();
        }
    }
}