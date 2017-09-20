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
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Sessions.Commands.BaseClasses;
using Inflatable.Sessions.Commands.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inflatable.Sessions.Commands
{
    /// <summary>
    /// Save command
    /// </summary>
    /// <seealso cref="CommandBaseClass"/>
    public class SaveCommand : CommandBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommand"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="objects">The objects.</param>
        public SaveCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, object[] objects)
            : base(mappingManager, queryProviderManager, objects)
        {
        }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public override CommandType CommandType => CommandType.Save;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>The number of rows that are modified.</returns>
        public override async Task<int> Execute(MappingSource source)
        {
            if (Objects.Length == 0)
                return 0;
            var ReturnValue = 0;
            var Batch = QueryProviderManager.CreateBatch(source.Source);
            var DeclarationBatch = QueryProviderManager.CreateBatch(source.Source);
            List<object> ObjectsSeen = new List<object>();
            for (int x = 0; x < Objects.Length; ++x)
            {
                Save(Objects[x], source, Batch, DeclarationBatch, ObjectsSeen);
            }
            if (!ObjectsSeen.Any())
                return 0;
            Batch = DeclarationBatch.RemoveDuplicateCommands().AddQuery(Batch);
            ReturnValue += await Batch.ExecuteScalarAsync<int>();
            Batch = Batch.CreateBatch();
            SaveJoins(source, Batch, ObjectsSeen);
            ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();

            return ReturnValue;
        }

        /// <summary>
        /// Setups the insert declarations.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        private static void SetupInsertDeclarations(IGenerator generator, SQLHelper.SQLHelper declarationBatch)
        {
            var DeclarationQuery = generator.GenerateDeclarations(QueryType.Insert);
            for (int x = 0; x < DeclarationQuery.Length; ++x)
            {
                declarationBatch.AddQuery(DeclarationQuery[x].QueryString, DeclarationQuery[x].DatabaseCommandType, DeclarationQuery[x].Parameters);
            }
        }

        /// <summary>
        /// Cascades the many to many properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeManyToManyProperties(object @object,
            MappingSource source,
            SQLHelper.SQLHelper batch,
            SQLHelper.SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> parentMappings)
        {
            IORMObject ORMObject = @object as IORMObject;
            foreach (var ManyToManyProperty in parentMappings.SelectMany(x => x.ManyToManyProperties)
                                                             .Where(x => x.Cascade
                                                                      && (ORMObject == null
                                                                         || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var ManyToManyValue = ManyToManyProperty.GetValue(@object) as IEnumerable;
                if (ManyToManyValue == null)
                    continue;
                foreach (var Item in ManyToManyValue)
                {
                    Save(Item, source, batch, declarationBatch, objectsSeen);
                }
            }
        }

        private void CascadeManyToOneProperties(object @object,
            MappingSource source,
            SQLHelper.SQLHelper batch,
            SQLHelper.SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> parentMappings)
        {
            IORMObject ORMObject = @object as IORMObject;
            foreach (var ManyToOneProperty in parentMappings.SelectMany(x => x.ManyToOneProperties)
                                                             .Where(x => x.Cascade
                                                                      && (ORMObject == null
                                                                         || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var ManyToOneValue = ManyToOneProperty.GetValue(@object);
                if (ManyToOneValue == null)
                    continue;
                var ManyToOneListValue = ManyToOneValue as IEnumerable;
                if (ManyToOneListValue == null)
                {
                    Save(ManyToOneValue, source, batch, declarationBatch, objectsSeen);
                }
                else
                {
                    foreach (var Item in ManyToOneListValue)
                    {
                        Save(Item, source, batch, declarationBatch, objectsSeen);
                    }
                }
            }
        }

        /// <summary>
        /// Cascades the map properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="ParentMappings">The parent mappings.</param>
        private void CascadeMapProperties(object @object,
            MappingSource source,
            SQLHelper.SQLHelper batch,
            SQLHelper.SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> ParentMappings)
        {
            IORMObject ORMObject = @object as IORMObject;
            foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties)
                                                              .Where(x => x.Cascade
                                                                       && (ORMObject == null
                                                                          || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var MapValue = MapProperty.GetValue(@object);
                Save(MapValue, source, batch, declarationBatch, objectsSeen);
            }
        }

        /// <summary>
        /// Inserts the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="idProperties">The identifier properties.</param>
        private void Insert(object @object, MappingSource source, SQLHelper.SQLHelper batch, SQLHelper.SQLHelper declarationBatch, IEnumerable<IIDProperty> idProperties)
        {
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            SetupInsertDeclarations(Generator, declarationBatch);
            var ObjectQueries = Generator.GenerateQueries(QueryType.Insert, @object);
            foreach (var ObjectQuery in ObjectQueries)
            {
                var IDProperty = idProperties.FirstOrDefault(y => y.AutoIncrement);
                var ReturnedID = batch.AddQuery((Command, ResultList, InsertObject) =>
                                                {
                                                    if (IDProperty != null && IDProperty.AutoIncrement)
                                                    {
                                                        IDProperty.SetValue(InsertObject, IDProperty.GetValue((Dynamo)ResultList[0]));
                                                    }
                                                },
                                                @object,
                                                ObjectQuery.QueryString,
                                                ObjectQuery.DatabaseCommandType,
                                                ObjectQuery.Parameters);
            }
        }

        /// <summary>
        /// Saves the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void Save(object @object, MappingSource source, SQLHelper.SQLHelper batch, SQLHelper.SQLHelper declarationBatch, IList<object> objectsSeen)
        {
            if (@object == null
                || WasObjectSeen(@object, objectsSeen, source)
                || !CanExecute(@object, source))
                return;
            objectsSeen.Add(@object);
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            var CurrentObjectType = @object.GetType();
            var ParentMappings = source.GetParentMapping(CurrentObjectType);

            CascadeMapProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);
            CascadeManyToManyProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);
            CascadeManyToOneProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);

            if (@object is IORMObject UpdateObject)
                Update(UpdateObject, source, batch);
            else
                Insert(@object, source, batch, declarationBatch, ParentMappings.SelectMany(x => x.IDProperties));

            RemoveItemsFromCache(@object);
        }

        /// <summary>
        /// Saves the joins.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void SaveJoins(MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen)
        {
            foreach (var TempObject in objectsSeen)
            {
                var ParentMappings = source.GetParentMapping(TempObject.GetType());
                foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, MapProperty);
                }
                foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, ManyToManyProperty);
                }
                foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToOneProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, ManyToManyProperty);
                }
            }
        }

        /// <summary>
        /// Saves the joins.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="property">The property.</param>
        private void SavePropertyJoins(object @object, MappingSource source, SQLHelper.SQLHelper batch, IClassProperty property)
        {
            var LinksGenerator = QueryProviderManager.CreateGenerator(property.ParentMapping.ObjectType, source);
            var TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsDelete, @object, property);
            foreach (var TempQuery in TempQueries)
            {
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
            TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsSave, @object, property);
            foreach (var TempQuery in TempQueries)
            {
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
        }

        /// <summary>
        /// Updates the specified update object.
        /// </summary>
        /// <param name="updateObject">The update object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        private void Update(IORMObject updateObject, MappingSource source, SQLHelper.SQLHelper batch)
        {
            var Generator = QueryProviderManager.CreateGenerator(updateObject.GetType(), source);
            var Queries = Generator.GenerateQueries(QueryType.Update, updateObject);
            foreach (var TempQuery in Queries)
            {
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
        }
    }
}