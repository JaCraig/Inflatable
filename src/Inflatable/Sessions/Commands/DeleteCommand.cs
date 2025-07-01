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
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Sessions.Commands.BaseClasses;
using SQLHelperDB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inflatable.Sessions.Commands
{
    /// <summary>
    /// Delete command
    /// </summary>
    /// <seealso cref="CommandBaseClass"/>
    public class DeleteCommand : CommandBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommand"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="objectsToDelete">The objects to delete.</param>
        public DeleteCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, ICache? cache, params object[] objectsToDelete)
            : base(mappingManager, queryProviderManager, cache, objectsToDelete)
        {
        }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public override Enums.CommandType CommandType => Enums.CommandType.Delete;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The number of rows that are modified.</returns>
        public override int Execute(IMappingSource source)
        {
            if (Objects.Length == 0)
            {
                return 0;
            }

            CreateBatch(source, out var Batch, out var ObjectsSeen);
            if (ObjectsSeen.Count == 0)
            {
                return 0;
            }

            return AsyncHelper.RunSync(() => Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>());
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">Mapping source.</param>
        /// <returns>The number of rows that are modified.</returns>
        public override Task<int> ExecuteAsync(IMappingSource source)
        {
            if (Objects.Length == 0)
            {
                return Task.FromResult(0);
            }

            CreateBatch(source, out var Batch, out var ObjectsSeen);
            if (ObjectsSeen.Count == 0)
            {
                return Task.FromResult(0);
            }

            return Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();
        }

        /// <summary>
        /// Cascades the many to many properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeManyToManyProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> parentMappings)
        {
            foreach (var ManyToManyProperty in parentMappings.SelectMany(x => x.ManyToManyProperties).Where(x => x.Cascade))
            {
                var ManyToManyValue = ManyToManyProperty.GetValue(@object);
                if (ManyToManyValue != null)
                {
                    if (ManyToManyValue is not IList ManyToManyValueList)
                        continue;
                    var FinalList = new List<object>();
                    for (int X = 0, ManyToManyValueListCount = ManyToManyValueList.Count; X < ManyToManyValueListCount; ++X)
                    {
                        var Item = ManyToManyValueList[X];
                        if (Item is null)
                            continue;
                        FinalList.Add(Item);
                    }
                    DeleteJoins(@object, source, batch, ManyToManyProperty, ManyToManyValueList);
                    for (int X = 0, FinalListCount = FinalList.Count; X < FinalListCount; ++X)
                    {
                        var Item = FinalList[X];
                        Delete(Item, source, batch, objectsSeen);
                    }
                }
            }
        }

        /// <summary>
        /// Cascades the many to one properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeManyToOneProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> parentMappings)
        {
            foreach (var ManyToOneProperty in parentMappings.SelectMany(x => x.ManyToOneProperties).Where(x => x.Cascade))
            {
                var ManyToOneValue = ManyToOneProperty.GetValue(@object);
                if (ManyToOneValue != null)
                {
                    if (ManyToOneValue is IList ManyToOneValueList)
                    {
                        var FinalList = new List<object>();
                        for (int X = 0, ManyToManyValueListCount = ManyToOneValueList.Count; X < ManyToManyValueListCount; ++X)
                        {
                            var Item = ManyToOneValueList[X];
                            if (Item is null)
                                continue;
                            FinalList.Add(Item);
                        }
                        for (int X = 0, FinalListCount = FinalList.Count; X < FinalListCount; ++X)
                        {
                            var Item = FinalList[X];
                            Delete(Item, source, batch, objectsSeen);
                        }
                    }
                    else
                    {
                        Delete(ManyToOneValue, source, batch, objectsSeen);
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
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        private void CascadeMapProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> parentMappings)
        {
            foreach (var MapProperty in parentMappings.SelectMany(x => x.MapProperties).Where(x => x.Cascade))
            {
                Delete(MapProperty.GetValue(@object), source, batch, objectsSeen);
            }
        }

        /// <summary>
        /// Creates the batch and gets the list of objects seen.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void CreateBatch(IMappingSource source, out SQLHelper batch, out List<object> objectsSeen)
        {
            batch = QueryProviderManager.CreateBatch(source.Source);
            objectsSeen = [];
            for (int X = 0, ObjectsLength = Objects.Length; X < ObjectsLength; ++X)
            {
                Delete(Objects[X], source, batch, objectsSeen);
            }
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void Delete(object? @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen)
        {
            if (@object is null
                || WasObjectSeen(@object, objectsSeen, source)
                || !CanExecute(@object, source))
            {
                return;
            }

            objectsSeen.Add(@object);
            DeleteCascade(@object, source, batch, objectsSeen);
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            var Queries = Generator?.GenerateQueries(QueryType.Delete, @object) ?? [];
            for (int X = 0, QueriesLength = Queries.Length; X < QueriesLength; X++)
            {
                var TempQuery = Queries[X];
                batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }

            RemoveItemsFromCache(@object);
        }

        /// <summary>
        /// Deletes the cascade.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void DeleteCascade(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen)
        {
            var ParentMappings = source.GetParentMapping(@object.GetType());
            CascadeMapProperties(@object, source, batch, objectsSeen, ParentMappings);
            CascadeManyToManyProperties(@object, source, batch, objectsSeen, ParentMappings);
            CascadeManyToOneProperties(@object, source, batch, objectsSeen, ParentMappings);
        }

        /// <summary>
        /// Deletes the joins if it needs to.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="manyToManyProperty">The many to many property.</param>
        /// <param name="manyToManyValueList">The many to many value list.</param>
        private void DeleteJoins(object @object, IMappingSource source, SQLHelper batch, IManyToManyProperty manyToManyProperty, IList manyToManyValueList)
        {
            if (manyToManyProperty.DatabaseJoinsCascade)
            {
                return;
            }

            manyToManyValueList.Clear();
            var LinksGenerator = QueryProviderManager.CreateGenerator(manyToManyProperty.ParentMapping.ObjectType, source);
            var TempQueries = LinksGenerator?.GenerateQueries(QueryType.JoinsDelete, @object, manyToManyProperty) ?? [];
            for (int X = 0, TempQueriesLength = TempQueries.Length; X < TempQueriesLength; ++X)
            {
                var TempQuery = TempQueries[X];
                batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }
        }
    }
}