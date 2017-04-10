using BigBook;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Data;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// Update query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
    public class UpdateQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        public UpdateQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Update;

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery()
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new Query(CommandType.Text, GenerateUpdateQuery(TypeGraph.Root), QueryType.Insert);
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The from clause</returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            foreach (var ParentNode in node.Nodes)
            {
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                StringBuilder IDProperties = new StringBuilder();
                string Separator = "";
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    IDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    IDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                Result.AppendLineFormat("INNER JOIN {0} ON {1}", GetTableName(ParentMapping), IDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the update query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string GenerateUpdateQuery(Utils.TreeNode<Type> node)
        {
            var Mapping = MappingInformation.Mappings[node.Data];
            if (Mapping.ReferenceProperties.Count == 0)
                return "";
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            string Splitter = "";

            //Generate parent queries
            foreach (var Parent in node.Nodes)
            {
                var Result = GenerateUpdateQuery(Parent);
                if (!string.IsNullOrEmpty(Result))
                {
                    Builder.AppendLine(Result);
                }
            }

            //Adding reference properties
            foreach (var ReferenceProperty in Mapping.ReferenceProperties)
            {
                ParameterList.Append(Splitter + GetColumnName(ReferenceProperty) + "=" + GetParameterName(ReferenceProperty));
                Splitter = ",";
            }

            //From clause generation
            FromClause.AppendLine(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node));

            //Where clause generation
            WhereClause.Append(GenerateWhereClause(node));

            //Generating final query
            Builder.AppendLineFormat(@"UPDATE {0}
SET {1}
FROM {2}WHERE {3};", GetTableName(Mapping), ParameterList, FromClause, WhereClause);

            return Builder.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The where clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateWhereClause(ParentNode);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator + ParentResult);
                    Separator = "\r\nAND ";
                }
            }
            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1}={2}", Separator, GetColumnName(IDProperty), GetParameterName(IDProperty));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }
    }
}