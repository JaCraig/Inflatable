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
    /// Many to one single side
    /// </summary>
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    public class ManyToOneSingle<ClassType, DataType> : ManyToOneOnePropertyBase<ClassType, DataType, ManyToOneSingle<ClassType, DataType>>
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToOneSingle{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ManyToOneSingle(Expression<Func<ClassType, DataType>> expression, IMapping mapping)
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
            var Result = new ExpressionTypeConverter<ClassType, DataType>(Expression).Convert<TResult>();
            var ReturnObject = new ManyToOneSingle<TResult, DataType>(Result, mapping);
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
            var TempColumns = new List<IQueryColumnInfo>();
            TempColumns.AddRange(ForeignMapping.SelectMany(TempMapping =>
            {
                return TempMapping?.IDProperties.ForEach(x =>
                {
                    return new ComplexColumnInfo<ClassType, DataType>(
                        x.GetColumnInfo()[0],
                        ColumnName + TempMapping.TableName + x.ColumnName,
                        CompiledExpression,
                        true,
                        ParentMapping.SchemaName,
                        ParentMapping.TableName
                    );
                });
            }));
            var ActualParent = mappings.GetChildMappings<ClassType>().SelectMany(x => mappings.GetParentMapping(x.ObjectType)).FirstOrDefault(x => x.IDProperties.Count > 0);
            TempColumns.AddRange(ActualParent.IDProperties.SelectMany(x => x.GetColumnInfo()));
            Columns = TempColumns.ToArray();
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dataModel">The data model.</param>
        /// <exception cref="ArgumentException"></exception>
        public override void Setup(MappingSource mappings, DataModel dataModel)
        {
            ForeignMapping = mappings.GetChildMappings<DataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .Where(x => x.IDProperties.Count > 0)
                                     .ToList();
            if (ForeignMapping == null)
            {
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}");
            }

            var ParentMappings = mappings.GetChildMappings(ParentMapping.ObjectType).SelectMany(x => mappings.GetParentMapping(x.ObjectType)).Distinct();
            var ActualParent = ParentMappings.FirstOrDefault(x => x.IDProperties.Count > 0);
            var ParentTable = dataModel.SourceSpec.Tables.Find(x => x.Name == ActualParent.TableName);
            foreach (var TempMapping in ForeignMapping)
            {
                var SetNullOnDelete = !ParentMappings.Contains(TempMapping);
                foreach (var IDMapping in TempMapping.IDProperties)
                {
                    if (ParentTable.Columns.Any(x => x.Name == ColumnName + TempMapping.TableName + IDMapping.ColumnName))
                    {
                        continue;
                    }

                    ParentTable.AddColumn<object>(ColumnName + TempMapping.TableName + IDMapping.ColumnName,
                                    IDMapping.PropertyType.To(DbType.Int32),
                                    IDMapping.MaxLength,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    TempMapping.TableName,
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