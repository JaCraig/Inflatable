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
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using ObjectCartographer;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.BaseClasses
{
    /// <summary>
    /// Generator base class
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="IQueryGenerator{TMappedClass}"/>
    public abstract class QueryGeneratorBaseClass<TMappedClass> : IQueryGenerator<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">mappingInformation</exception>
        protected QueryGeneratorBaseClass(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
        {
            MappingInformation = mappingInformation ?? throw new ArgumentNullException(nameof(mappingInformation));
            ObjectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
        }

        /// <summary>
        /// Gets the type of the associated.
        /// </summary>
        /// <value>The type of the associated.</value>
        public Type AssociatedType => typeof(TMappedClass);

        /// <summary>
        /// Gets the mapping information.
        /// </summary>
        /// <value>The mapping information.</value>
        public IMappingSource MappingInformation { get; }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public abstract QueryType QueryType { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public abstract IQuery[] GenerateDeclarations();

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public abstract IQuery[] GenerateQueries(TMappedClass queryObject);

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IIDProperty? idProperty, string suffix = "") => GetTableName(idProperty?.ParentMapping, suffix) + ".[" + idProperty?.ColumnName + "]";

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IAutoIDProperty idProperty, string suffix = "") => GetTableName(idProperty?.ParentMapping, suffix) + ".[" + idProperty?.ColumnName + "]";

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="referenceProperty">The reference property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IProperty referenceProperty, string suffix = "") => GetTableName(referenceProperty?.ParentMapping, suffix) + ".[" + referenceProperty?.ColumnName + "]";

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="mapProperty">The map property.</param>
        /// <param name="foreignMapping">The foreign mapping.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The column name</returns>
        protected string? GetColumnName(IMapProperty mapProperty, IMapping foreignMapping, string suffix = "")
        {
            return foreignMapping?.IDProperties.ToString(x => GetTableName(mapProperty.ParentMapping, suffix)
                                                                        + ".[" + foreignMapping.TableName
                                                                        + mapProperty.ParentMapping.Prefix
                                                                        + mapProperty.Name
                                                                        + mapProperty.ParentMapping.Suffix
                                                                        + x.ColumnName + "]");
        }

        /// <summary>
        /// Gets the name of the parent column.
        /// </summary>
        /// <param name="foreignMapping">The foreign mapping.</param>
        /// <returns>The parent column name</returns>
        protected string GetForeignColumnName(IMapping foreignMapping)
        {
            return GetColumnName(foreignMapping?.IDProperties.First());
        }

        /// <summary>
        /// Gets the name of the parent parameter.
        /// </summary>
        /// <param name="foreignMapping">The foreign mapping.</param>
        /// <returns>The parent parameter name</returns>
        protected string GetForeignParameterName(IMapping foreignMapping)
        {
            return GetParameterName(foreignMapping?.IDProperties.First());
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <returns>The parameter name</returns>
        protected string GetParameterName(IIDProperty? idProperty) => $"@{idProperty?.Name}";

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="mapProperty">The map property.</param>
        /// <param name="foreignMapping">The foreign mapping.</param>
        /// <returns>The parameter name</returns>
        protected string? GetParameterName(IMapProperty mapProperty, IMapping foreignMapping)
        {
            return foreignMapping?.IDProperties.ToString(x => "@" + foreignMapping.TableName
                                                                        + mapProperty.ParentMapping.Prefix
                                                                        + mapProperty.Name
                                                                        + mapProperty.ParentMapping.Suffix
                                                                        + x.ColumnName);
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="referenceProperty">The reference property.</param>
        /// <returns>The parameter name</returns>
        protected string GetParameterName(IProperty referenceProperty) => $"@{referenceProperty?.Name}";

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <returns>The parameter type name</returns>
        protected string GetParameterType(IAutoIDProperty autoIDProperty) => "BIGINT";

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <param name="iDProperty">The i d property.</param>
        /// <returns>The parameter type name</returns>
        protected string GetParameterType(IIDProperty iDProperty) => iDProperty?.PropertyType.To<SqlDbType>().ToString().ToUpper(CultureInfo.InvariantCulture) ?? string.Empty;

        /// <summary>
        /// Gets the name of the parent column.
        /// </summary>
        /// <param name="childMapping">The child mapping.</param>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The parent column name</returns>
        protected string GetParentColumnName(IMapping childMapping, IAutoIDProperty autoIDProperty, string suffix = "") => $"{GetTableName(childMapping, suffix)}.[{autoIDProperty?.ParentMapping.TableName}{autoIDProperty?.ColumnName}]";

        /// <summary>
        /// Gets the name of the parent column.
        /// </summary>
        /// <param name="childMapping">The child mapping.</param>
        /// <param name="iDProperty">The i d property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The parent column name</returns>
        protected string GetParentColumnName(IMapping childMapping, IIDProperty iDProperty, string suffix = "") => $"{GetTableName(childMapping, suffix)}.[{iDProperty?.ParentMapping.TableName}{iDProperty?.ColumnName}]";

        /// <summary>
        /// Gets the name of the parent parameter.
        /// </summary>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <returns>The parent parameter name</returns>
        protected string GetParentParameterName(IAutoIDProperty autoIDProperty) => $"@{autoIDProperty?.ParentMapping.TableName}{autoIDProperty?.ColumnName}Temp";

        /// <summary>
        /// Gets the name of the parent parameter.
        /// </summary>
        /// <param name="iDProperty">The i d property.</param>
        /// <returns>The parent parameter name</returns>
        protected string GetParentParameterName(IIDProperty iDProperty) => $"@{iDProperty?.ParentMapping.TableName}{iDProperty?.Name}_Temp";

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The name of the table</returns>
        protected string GetTableName(IMapping? parentMapping, string suffix = "")
        {
            if (parentMapping is null)
                return string.Empty;
            return string.IsNullOrEmpty(suffix)
                ? "[" + parentMapping.SchemaName + "].[" + parentMapping.TableName + "]"
                : "[" + parentMapping.TableName + suffix + "]";
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The name of the table.</returns>
        protected string GetTableName(IManyToManyProperty property) => $"[{property?.ParentMapping.SchemaName}].[{property.TableName}]";
    }
}