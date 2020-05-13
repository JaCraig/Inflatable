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
using BigBook.Caching.Interfaces;
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
        public DeleteCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, ICache cache, params object[] objectsToDelete)
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
        public override int Execute(IMappingSource source, DynamoFactory dynamoFactory)
        {
            if (Objects.Length == 0)
            {
                return 0;
            }

            CreateBatch(source, dynamoFactory, out var Batch, out var ObjectsSeen);
            if (ObjectsSeen.Count == 0)
            {
                return 0;
            }

            return Task.Run(async () => await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>().ConfigureAwait(false)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">Mapping source.</param>
        /// <param name="dynamoFactory">The dynamo factory.</param>
        /// <returns>The number of rows that are modified.</returns>
        public override async Task<int> ExecuteAsync(IMappingSource source, DynamoFactory dynamoFactory)
        {
            if (Objects.Length == 0)
            {
                return 0;
            }

            CreateBatch(source, dynamoFactory, out var Batch, out var ObjectsSeen);
            if (ObjectsSeen.Count == 0)
            {
                return 0;
            }

            return await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        /// <summary>
        /// Cascades the many to many properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="ParentMappings">The parent mappings.</param>
        private void CascadeManyToManyProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties).Where(x => x.Cascade))
            {
                var ManyToManyValue = ManyToManyProperty.GetValue(@object);
                if (ManyToManyValue != null)
                {
                    if (!(ManyToManyValue is IList ManyToManyValueList))
                        continue;
                    var FinalList = new List<object>();
                    for (int x = 0, ManyToManyValueListCount = ManyToManyValueList.Count; x < ManyToManyValueListCount; ++x)
                    {
                        var Item = ManyToManyValueList[x];
                        FinalList.Add(Item);
                    }
                    DeleteJoins(@object, source, batch, ManyToManyProperty, ManyToManyValueList);
                    for (int x = 0, FinalListCount = FinalList.Count; x < FinalListCount; ++x)
                    {
                        var Item = FinalList[x];
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
        /// <param name="ParentMappings">The parent mappings.</param>
        private void CascadeManyToOneProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var ManyToOneProperty in ParentMappings.SelectMany(x => x.ManyToOneProperties).Where(x => x.Cascade))
            {
                var ManyToOneValue = ManyToOneProperty.GetValue(@object);
                if (ManyToOneValue != null)
                {
                    if (ManyToOneValue is IList ManyToOneValueList)
                    {
                        var FinalList = new List<object>();
                        for (int x = 0, ManyToManyValueListCount = ManyToOneValueList.Count; x < ManyToManyValueListCount; ++x)
                        {
                            var Item = ManyToOneValueList[x];
                            FinalList.Add(Item);
                        }
                        for (int x = 0, FinalListCount = FinalList.Count; x < FinalListCount; ++x)
                        {
                            var Item = FinalList[x];
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
        /// <param name="ParentMappings">The parent mappings.</param>
        private void CascadeMapProperties(object @object, IMappingSource source, SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties).Where(x => x.Cascade))
            {
                Delete(MapProperty.GetValue(@object), source, batch, objectsSeen);
            }
        }

        /// <summary>
        /// Creates the batch and gets the list of objects seen.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dynamoFactory">The dynamo factory.</param>
        /// <param name="Batch">The batch.</param>
        /// <param name="ObjectsSeen">The objects seen.</param>
        private void CreateBatch(IMappingSource source, DynamoFactory dynamoFactory, out SQLHelper Batch, out List<object> ObjectsSeen)
        {
            Batch = QueryProviderManager.CreateBatch(source.Source, dynamoFactory);
            ObjectsSeen = new List<object>();
            for (int x = 0, ObjectsLength = Objects.Length; x < ObjectsLength; ++x)
            {
                Delete(Objects[x], source, Batch, ObjectsSeen);
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
            var Queries = Generator.GenerateQueries(QueryType.Delete, @object);
            for (int x = 0, QueriesLength = Queries.Length; x < QueriesLength; x++)
            {
                var TempQuery = Queries[x];
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
        /// <param name="ManyToManyProperty">The many to many property.</param>
        /// <param name="ManyToManyValueList">The many to many value list.</param>
        private void DeleteJoins(object @object, IMappingSource source, SQLHelper batch, IManyToManyProperty ManyToManyProperty, IList ManyToManyValueList)
        {
            if (ManyToManyProperty.DatabaseJoinsCascade)
            {
                return;
            }

            ManyToManyValueList.Clear();
            var LinksGenerator = QueryProviderManager.CreateGenerator(ManyToManyProperty.ParentMapping.ObjectType, source);
            var TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsDelete, @object, ManyToManyProperty);
            for (int x = 0, TempQueriesLength = TempQueries.Length; x < TempQueriesLength; ++x)
            {
                var TempQuery = TempQueries[x];
                batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
            }
        }
    }
}