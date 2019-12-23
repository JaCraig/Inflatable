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
using Inflatable.ClassMapper.BaseClasses;
using Inflatable.ClassMapper.Column;
using Inflatable.ClassMapper.Column.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.Schema;
using Inflatable.Utils;
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
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    public class ManyToOneMany<ClassType, DataType> : ManyToOneManyPropertyBase<ClassType, DataType, ManyToOneMany<ClassType, DataType>>
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToOneMany{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ManyToOneMany(Expression<Func<ClassType, IList<DataType>>> expression, IMapping mapping)
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
            var Result = new ExpressionTypeConverter<ClassType, IList<DataType>>
            {
                Expression = Expression
            }.Convert<TResult>();
            var ReturnObject = new ManyToOneMany<TResult, DataType>(Result, mapping);
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
        public override void SetColumnInfo(MappingSource mappings)
        {
            var ActualParent = mappings.GetChildMappings<ClassType>().SelectMany(x => mappings.GetParentMapping(x.ObjectType)).FirstOrDefault(x => x.IDProperties.Count > 0);
            var TempColumns = new List<IQueryColumnInfo>();
            TempColumns.AddRange(ActualParent.IDProperties.ForEach(x =>
            {
                var IDColumnInfo = x.GetColumnInfo()[0];
                return new ComplexColumnInfo<ClassType, ClassType>
                {
                    Child = IDColumnInfo,
                    ColumnName = ColumnName + x.ParentMapping.TableName + x.ColumnName,
                    CompiledExpression = y => y,
                    SchemaName = ForeignMapping.SchemaName,
                    TableName = ForeignMapping.TableName,
                    IsForeign = true
                };
            }));
            TempColumns.AddRange(ForeignMapping.IDProperties.ForEach(x =>
            {
                var IDColumnInfo = x.GetColumnInfo()[0];
                return new ComplexListColumnInfo<ClassType, DataType>
                {
                    Child = IDColumnInfo,
                    ColumnName = x.ColumnName,
                    CompiledExpression = CompiledExpression,
                    SchemaName = x.ParentMapping.SchemaName,
                    TableName = x.ParentMapping.TableName,
                    IsForeign = false
                };
            }));
            Columns = TempColumns.ToArray();
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dataModel">The data model.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public override void Setup(MappingSource mappings, DataModel dataModel)
        {
            ForeignMapping = mappings.GetChildMappings<DataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .FirstOrDefault(x => x.IDProperties.Count > 0);
            if (ForeignMapping == null)
            {
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}");
            }

            var ForeignTable = dataModel.SourceSpec.Tables.FirstOrDefault(x => x.Name == ForeignMapping.TableName);
            var ParentMappings = mappings.GetChildMappings(ParentMapping.ObjectType).SelectMany(x => mappings.GetParentMapping(x.ObjectType)).Distinct();
            var ParentIDs = ParentMappings.SelectMany(x => x.IDProperties);
            var SetNullOnDelete = !ParentMappings.Contains(ForeignMapping);
            foreach (var IDMapping in ParentIDs)
            {
                if (ForeignTable.Columns.Any(x => x.Name == ColumnName + IDMapping.ParentMapping.TableName + IDMapping.ColumnName))
                {
                    continue;
                }

                ForeignTable.AddColumn<object>(ColumnName + IDMapping.ParentMapping.TableName + IDMapping.ColumnName,
                                IDMapping.PropertyType.To(DbType.Int32),
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