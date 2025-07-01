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
using DragonHoard.Core.Interfaces;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Sessions.Commands.BaseClasses;
using Inflatable.Sessions.Commands.Enums;
using SQLHelperDB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Valkyrie.ExtensionMethods;

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
        /// <param name="cache">The cache.</param>
        /// <param name="objects">The objects.</param>
        public SaveCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, ICache? cache, object[] objects)
            : base(mappingManager, queryProviderManager, cache, objects)
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
        /// <param name="source">The source.</param>
        /// <returns>The number of rows that are modified.</returns>
        public override int Execute(IMappingSource source)
        {
            return AsyncHelper.RunSync(() => ExecuteAsync(source));
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The number of rows that are modified.</returns>
        public override async Task<int> ExecuteAsync(IMappingSource source)
        {
            if (Objects.Length == 0)
            {
                return 0;
            }

            var ReturnValue = 0;
            CreateBatch(source, out var Batch, out var DeclarationBatch, out var ObjectsSeen);
            if (ObjectsSeen.Count == 0)
            {
                return 0;
            }

            ValidateObjects(ObjectsSeen);
            Batch = DeclarationBatch.RemoveDuplicateCommands().AddQuery(Batch);
            ReturnValue += await Batch.ExecuteScalarAsync<int>().ConfigureAwait(false);
            Batch = QueryProviderManager.CreateBatch(source.Source);
            DeclarationBatch = QueryProviderManager.CreateBatch(source.Source);
            SaveJoins(source, Batch, DeclarationBatch, ObjectsSeen);
            Batch = DeclarationBatch.RemoveDuplicateCommands().AddQuery(Batch);
            ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>().ConfigureAwait(false);

            return ReturnValue;
        }

        /// <summary>
        /// Setups the insert declarations.
        /// </summary>
        /// <param name="generator">The generator.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        private static void SetupInsertDeclarations(IGenerator? generator, SQLHelper declarationBatch)
        {
            var DeclarationQuery = generator?.GenerateDeclarations(QueryType.Insert);
            if (DeclarationQuery is null || DeclarationQuery.Length == 0)
                return;
            for (int X = 0, DeclarationQueryLength = DeclarationQuery.Length; X < DeclarationQueryLength; ++X)
            {
                var CurrentDeclarationQuery = DeclarationQuery[X];
                declarationBatch.AddHeader(CurrentDeclarationQuery.DatabaseCommandType, CurrentDeclarationQuery.QueryString, CurrentDeclarationQuery.Parameters!);
            }
        }

        /// <summary>
        /// Validates the objects.
        /// </summary>
        /// <param name="objectsSeen">The objects seen.</param>
        private static void ValidateObjects(List<object> objectsSeen)
        {
            for (int X = 0, ObjectsSeenLength = objectsSeen.Count; X < ObjectsSeenLength; ++X)
            {
                objectsSeen[X].Validate();
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
            IMappingSource source,
            SQLHelper batch,
            SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> parentMappings)
        {
            var ORMObject = @object as IORMObject;
            foreach (var ManyToManyProperty in parentMappings.SelectMany(x => x.ManyToManyProperties)
                                                             .Where(x => x.Cascade
                                                                      && (ORMObject?.PropertiesChanged0.Contains(x.Name) != false)))
            {
                if (ManyToManyProperty.GetValue(@object) is not IEnumerable ManyToManyValue)
                {
                    continue;
                }

                foreach (var Item in ManyToManyValue)
                {
                    Save(Item, source, batch, declarationBatch, objectsSeen);
                }
            }
        }

        /// <summary>
        /// Cascades the many to one properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeManyToOneProperties(object @object,
            IMappingSource source,
            SQLHelper batch,
            SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> parentMappings)
        {
            var ORMObject = @object as IORMObject;
            foreach (var ManyToOneProperty in parentMappings.SelectMany(x => x.ManyToOneProperties)
                                                             .Where(x => x.Cascade
                                                                      && (ORMObject?.PropertiesChanged0.Contains(x.Name) != false)))
            {
                var ManyToOneValue = ManyToOneProperty.GetValue(@object);
                if (ManyToOneValue is null)
                {
                    continue;
                }

                if (ManyToOneValue is not IEnumerable ManyToOneListValue)
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
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeMapProperties(object @object,
            IMappingSource source,
            SQLHelper batch,
            SQLHelper declarationBatch,
            IList<object> objectsSeen,
            IEnumerable<IMapping> parentMappings)
        {
            var ORMObject = @object as IORMObject;
            foreach (var MapProperty in parentMappings.SelectMany(x => x.MapProperties)
                                                              .Where(x => x.Cascade
                                                                       && (ORMObject?.PropertiesChanged0.Contains(x.Name) != false)))
            {
                var MapValue = MapProperty.GetValue(@object);
                Save(MapValue, source, batch, declarationBatch, objectsSeen);
            }
        }

        /// <summary>
        /// Creates the batch.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="declarationBatch">The declaration batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void CreateBatch(IMappingSource source, out SQLHelper batch, out SQLHelper declarationBatch, out List<object> objectsSeen)
        {
            batch = QueryProviderManager.CreateBatch(source.Source);
            declarationBatch = QueryProviderManager.CreateBatch(source.Source);
            objectsSeen = [];
            for (int X = 0, ObjectsLength = Objects.Length; X < ObjectsLength; ++X)
            {
                Save(Objects[X], source, batch, declarationBatch, objectsSeen);
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
        private void Insert(object @object, IMappingSource source, SQLHelper batch, SQLHelper declarationBatch, IEnumerable<IIDProperty> idProperties)
        {
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            SetupInsertDeclarations(Generator, declarationBatch);
            if (Generator is null)
                return;
            var ObjectQueries = Generator.GenerateQueries(QueryType.Insert, @object);
            for (int X = 0, ObjectQueriesLength = ObjectQueries.Length; X < ObjectQueriesLength; ++X)
            {
                var ObjectQuery = ObjectQueries[X];
                var IDProperty = idProperties.FirstOrDefault(y => y.AutoIncrement);
                var ReturnedID = batch.AddQuery((_, resultList, insertObject) =>
                {
                    if (IDProperty?.AutoIncrement == true)
                    {
                        IDProperty.GetColumnInfo()[0].SetValue(insertObject, IDProperty.GetColumnInfo()[0].GetValue((Dynamo)resultList[0])!);
                    }
                },
                                                @object,
                                                ObjectQuery.DatabaseCommandType,
                                                ObjectQuery.QueryString,
                                                ObjectQuery.Parameters!);
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
        private void Save(object? @object, IMappingSource source, SQLHelper batch, SQLHelper declarationBatch, IList<object> objectsSeen)
        {
            if (@object is null
                || WasObjectSeen(@object, objectsSeen, source)
                || !CanExecute(@object, source))
            {
                return;
            }

            objectsSeen.Add(@object);
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            var CurrentObjectType = @object.GetType();
            var ParentMappings = source.GetParentMapping(CurrentObjectType);

            CascadeMapProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);
            CascadeManyToManyProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);
            CascadeManyToOneProperties(@object, source, batch, declarationBatch, objectsSeen, ParentMappings);

            if (@object is IORMObject UpdateObject)
            {
                Update(UpdateObject, source, batch);
            }
            else
            {
                var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                var IsUpdatable = IDProperties.Any() && IDProperties.All(y => y.AutoIncrement && y.GetColumnInfo().All(z => !z.IsDefault(@object)));
                if (IsUpdatable)
                {
                    Update(@object, source, batch);
                }
                else
                {
                    Insert(@object, source, batch, declarationBatch, ParentMappings.SelectMany(x => x.IDProperties));
                }
            }

            RemoveItemsFromCache(@object);
        }

        /// <summary>
        /// Saves the joins.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="deleteBatch">The delete batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void SaveJoins(IMappingSource source, SQLHelper batch, SQLHelper deleteBatch, List<object> objectsSeen)
        {
            for (int I = 0, ObjectsSeenCount = objectsSeen.Count; I < ObjectsSeenCount; I++)
            {
                var TempObject = objectsSeen[I];
                var ParentMappings = source.GetParentMapping(TempObject.GetType());
                foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, deleteBatch, MapProperty);
                }
                foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, deleteBatch, ManyToManyProperty);
                }
                foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToOneProperties))
                {
                    SavePropertyJoins(TempObject, source, batch, deleteBatch, ManyToManyProperty);
                }
            }
        }

        /// <summary>
        /// Saves the joins.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="deleteBatch">The delete batch.</param>
        /// <param name="property">The property.</param>
        private void SavePropertyJoins(object @object, IMappingSource source, SQLHelper batch, SQLHelper deleteBatch, IClassProperty property)
        {
            var ORMObject = @object as IORMObject;
            if (!(ORMObject?.PropertiesChanged0.Contains(property.Name) ?? true))
                return;
            var LinksGenerator = QueryProviderManager.CreateGenerator(property.ParentMapping.ObjectType, source);
            if (LinksGenerator is null)
                return;
            var TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsDelete, @object, property) ?? [];
            for (int X = 0, TempQueriesLength = TempQueries.Length; X < TempQueriesLength; X++)
            {
                var TempQuery = TempQueries[X];
                deleteBatch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }

            TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsSave, @object, property) ?? [];
            for (int X = 0, TempQueriesLength = TempQueries.Length; X < TempQueriesLength; X++)
            {
                var TempQuery = TempQueries[X];
                batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }
        }

        /// <summary>
        /// Updates the specified update object.
        /// </summary>
        /// <param name="updateObject">The update object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        private void Update(object updateObject, IMappingSource source, SQLHelper batch)
        {
            var Generator = QueryProviderManager.CreateGenerator(updateObject.GetType(), source);
            var Queries = Generator?.GenerateQueries(QueryType.Update, updateObject) ?? [];
            for (int X = 0, QueriesLength = Queries.Length; X < QueriesLength; X++)
            {
                var TempQuery = Queries[X];
                batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }
        }
    }
}