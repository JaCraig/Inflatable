﻿/*
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
using SQLHelper.HelperClasses.Interfaces;
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
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
    public class InsertQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        public InsertQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            QueryDeclarationText = GenerateInsertQueryDeclarations();
            QueryText = GenerateInsertQuery(TypeGraph.Root);
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
        private IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the query declaration text.
        /// </summary>
        /// <value>The query declaration text.</value>
        private string[] QueryDeclarationText { get; set; }

        /// <summary>
        /// Gets or sets the query text.
        /// </summary>
        /// <value>The query text.</value>
        private string QueryText { get; set; }

        /// <summary>
        /// Gets or sets the reference properties.
        /// </summary>
        /// <value>The reference properties.</value>
        private IEnumerable<IProperty> ReferenceProperties { get; set; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations()
        {
            List<IQuery> ReturnValue = new List<IQuery>();
            for (int x = 0; x < QueryDeclarationText.Length; ++x)
            {
                ReturnValue.Add(new Query(AssociatedType, CommandType.Text, QueryDeclarationText[x], QueryType));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Generates the insert query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject)
        {
            return new IQuery[] { new Query(AssociatedType, CommandType.Text, QueryText, QueryType, GenerateParameters(queryObject)) };
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

        /// <summary>
        /// Generates the insert query declarations.
        /// </summary>
        /// <returns>The resulting query</returns>
        private string[] GenerateInsertQueryDeclarations()
        {
            List<string> Builder = new List<string>();
            List<IMapping> ParentMappings = new List<IMapping>();
            var ChildMappings = MappingInformation.GetChildMappings(typeof(TMappedClass));
            foreach (var ChildMapping in ChildMappings)
            {
                var TempParentMappings = MappingInformation.GetParentMapping(ChildMapping.ObjectType);
                ParentMappings.AddIfUnique(TempParentMappings);
            }
            foreach (var ParentMapping in ParentMappings)
            {
                //ID Properties to pass to the next set of queries
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    Builder.Add("DECLARE " + GetParentParameterName(IDProperty) + " AS " + GetParameterType(IDProperty) + ";");
                }

                //Auto ID properties to pass to the next set of queries
                foreach (var AutoIDProperty in ParentMapping.AutoIDProperties)
                {
                    Builder.Add("DECLARE " + GetParentParameterName(AutoIDProperty) + " AS " + GetParameterType(AutoIDProperty) + ";");
                }
            }

            return Builder.ToArray();
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters</returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject)
        {
            var Parameters = IDProperties.ForEach(y => y.GetAsParameter(queryObject)).ToList();
            Parameters.AddRange(ReferenceProperties.ForEach(y => y.GetAsParameter(queryObject)));
            return Parameters.ToArray();
        }
    }
}