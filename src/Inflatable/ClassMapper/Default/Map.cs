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
using Inflatable.ClassMapper.BaseClasses;
using Inflatable.ClassMapper.Column;
using Inflatable.ClassMapper.Column.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Map property
    /// </summary>
    /// <typeparam name="TClassType">The type of the class type.</typeparam>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    /// <seealso cref="IMapProperty"/>
    public class Map<TClassType, TDataType> : SingleClassPropertyBase<TClassType, TDataType, Map<TClassType, TDataType>>
        where TClassType : class
        where TDataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public Map(Expression<Func<TClassType, TDataType?>> expression, IMapping mapping) : base(expression, mapping)
        {
            if (IEnumerableType.IsAssignableFrom(typeof(TDataType)))
            {
                throw new ArgumentException("Expression is an IEnumerable, use ManyToOne or ManyToMany instead");
            }
        }

        /// <summary>
        /// Gets the type of the ienumerable.
        /// </summary>
        /// <value>The type of the ienumerable.</value>
        private static Type IEnumerableType { get; } = typeof(IEnumerable);

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IMapProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<TClassType, TDataType?>(Expression).Convert<TResult>();
            var ReturnObject = new Map<TResult, TDataType>(Result, mapping);
            if (Cascade)
            {
                ReturnObject.CascadeChanges();
            }

            if (LoadPropertyQuery != null)
            {
                ReturnObject.LoadUsing(LoadPropertyQuery.QueryString, LoadPropertyQuery.DatabaseCommandType);
            }

            return ReturnObject;
        }

        /// <summary>
        /// Sets the column information.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public override void SetColumnInfo(IMappingSource mappings)
        {
            if (mappings is null)
                return;
            var TempColumns = new List<IQueryColumnInfo>();
            TempColumns.AddRange(ForeignMapping.SelectMany(TempMapping =>
            {
                return TempMapping.IDProperties.ForEach(x =>
                {
                    var IDColumnInfo = x.GetColumnInfo()[0];
                    return new ComplexColumnInfo<TClassType, TDataType>(
                        IDColumnInfo,
                        TempMapping.TableName + ParentMapping.Prefix + Name + ParentMapping.Suffix + IDColumnInfo.ColumnName,
                        CompiledExpression,
                        true,
                        ParentMapping.SchemaName,
                        ParentMapping.TableName
                    );
                });
            }));
            TempColumns.AddRange(mappings.GetChildMappings(ParentMapping.ObjectType)
                                         .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                         .Distinct()
                                         .SelectMany(x => x.IDProperties)
                                         .SelectMany(x => x.GetColumnInfo()));
            Columns = [.. TempColumns];
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings"></param>
        public override void Setup(IMappingSource mappings)
        {
            if (mappings is null)
                return;
            ForeignMapping = [.. mappings.GetChildMappings<TDataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .Where(x => x.IDProperties.Count > 0)
                                     .Distinct()];
            if (ForeignMapping is null)
            {
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(TClassType).Name}.{Name}");
            }

            var ParentMappings = mappings.GetChildMappings(ParentMapping.ObjectType).SelectMany(x => mappings.GetParentMapping(x.ObjectType)).Distinct();
            SetNullOnDelete = !OnDeleteDoNothingValue && ForeignMapping.Any(x => !ParentMappings.Contains(x));
        }
    }
}