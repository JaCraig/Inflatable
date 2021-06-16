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
using Data.Modeler.Providers.Interfaces;
using Inflatable.ClassMapper.BaseClasses;
using Inflatable.ClassMapper.Column;
using Inflatable.ClassMapper.Column.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.Utils;
using ObjectCartographer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Many to one Many side
    /// </summary>
    /// <typeparam name="TClassType">The type of the lass type.</typeparam>
    /// <typeparam name="TDataType">The type of the ata type.</typeparam>
    public class ManyToOneMany<TClassType, TDataType> : ManyToOneManyPropertyBase<TClassType, TDataType, ManyToOneMany<TClassType, TDataType>>
        where TClassType : class
        where TDataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToOneMany{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ManyToOneMany(Expression<Func<TClassType, IList<TDataType>>> expression, IMapping mapping)
            : base(expression, mapping)
        {
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IManyToOneProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<TClassType, IList<TDataType>>(Expression).Convert<TResult>();
            var ReturnObject = new ManyToOneMany<TResult, TDataType>(Result, mapping);
            if (Cascade)
            {
                ReturnObject.CascadeChanges();
            }

            if (LoadPropertyQuery != null)
            {
                ReturnObject.LoadUsing(LoadPropertyQuery.QueryString, LoadPropertyQuery.DatabaseCommandType);
            }

            if (!string.IsNullOrEmpty(ColumnName))
            {
                ReturnObject.SetColumnName(ColumnName);
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
            var ActualParent = mappings.GetChildMappings<TClassType>().SelectMany(x => mappings.GetParentMapping(x.ObjectType)).FirstOrDefault(x => x.IDProperties.Count > 0);
            var TempColumns = new List<IQueryColumnInfo>();
            TempColumns.AddRange(ForeignMapping.SelectMany(TempMapping =>
            {
                return ActualParent.IDProperties.ForEach(x =>
                {
                    return new ComplexColumnInfo<TClassType, TClassType>(
                        x.GetColumnInfo()[0],
                        ColumnName + x.ParentMapping.TableName + x.ColumnName,
                        y => y,
                        true,
                        TempMapping?.SchemaName ?? string.Empty,
                        TempMapping?.TableName ?? string.Empty
                    );
                });
            }));
            TempColumns.AddRange(ForeignMapping.SelectMany(TempMapping =>
            {
                return TempMapping?.IDProperties.ForEach(x =>
                {
                    return new ComplexListColumnInfo<TClassType, TDataType>(
                        x.GetColumnInfo()[0],
                        x.ColumnName,
                        CompiledExpression,
                        false,
                        x.ParentMapping.SchemaName,
                        x.ParentMapping.TableName
                    );
                });
            }));
            Columns = TempColumns.ToArray();
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="sourceSpec">The source spec.</param>
        /// <exception cref="ArgumentException">Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}</exception>
        public override void Setup(IMappingSource mappings, ISource sourceSpec)
        {
            if (sourceSpec is null || mappings is null)
                return;
            ForeignMapping = mappings.GetChildMappings<TDataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .Where(x => x.IDProperties.Count > 0)
                                     .Distinct()
                                     .ToList();
            if (ForeignMapping is null)
            {
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(TClassType).Name}.{Name}");
            }

            foreach (var TempMapping in ForeignMapping)
            {
                var ForeignTable = sourceSpec.Tables.Find(x => x.Name == TempMapping.TableName);
                var ParentMappings = mappings.GetChildMappings(ParentMapping.ObjectType).SelectMany(x => mappings.GetParentMapping(x.ObjectType)).Distinct();
                var ParentIDs = ParentMappings.SelectMany(x => x.IDProperties);
                var SetNullOnDelete = !ParentMappings.Contains(TempMapping);
                foreach (var IDMapping in ParentIDs)
                {
                    if (ForeignTable.Columns.Any(x => x.Name == ColumnName + IDMapping.ParentMapping.TableName + IDMapping.ColumnName))
                    {
                        continue;
                    }

                    ForeignTable.AddColumn<object>(ColumnName + IDMapping.ParentMapping.TableName + IDMapping.ColumnName,
                                    IDMapping.PropertyType.To<DbType>(),
                                    IDMapping.MaxLength,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    IDMapping.ParentMapping.TableName,
                                    IDMapping.ColumnName,
                                    null!,
                                    "",
                                    false,
                                    false,
                                    !OnDeleteDoNothingValue && SetNullOnDelete);
                }
            }
        }
    }
}