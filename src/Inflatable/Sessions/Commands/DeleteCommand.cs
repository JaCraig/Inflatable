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
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Sessions.Commands.BaseClasses;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inflatable.Sessions.Commands
{
    /// <summary>
    /// Delete command
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="BaseClasses.CommandBaseClass{TObject}"/>
    public class DeleteCommand<TObject> : CommandBaseClass<TObject>
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommand{TObject}"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="objectsToDelete">The objects to delete.</param>
        public DeleteCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, params TObject[] objectsToDelete)
            : base(mappingManager, queryProviderManager, objectsToDelete)
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
        /// <returns>The number of rows that are modified.</returns>
        public override async Task<int> Execute()
        {
            if (Objects.Length == 0)
                return 0;
            var ReturnValue = 0;
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                for (int x = 0; x < Objects.Length; ++x)
                {
                    Delete(Objects[x], Source, Batch, new List<object>());
                }
                ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();
            }

            return ReturnValue;
        }

        /// <summary>
        /// Cascades the many to many properties.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="ParentMappings">The parent mappings.</param>
        private void CascadeManyToManyProperties(object @object, MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties).Where(x => x.Cascade))
            {
                var ManyToManyValue = ManyToManyProperty.GetValue(@object);
                if (ManyToManyValue != null)
                {
                    var ManyToManyValueList = ManyToManyValue as IList;
                    List<object> FinalList = new List<object>();
                    int ManyToManyValueListCount = ManyToManyValueList.Count;
                    for (int x = 0; x < ManyToManyValueListCount; ++x)
                    {
                        var Item = ManyToManyValueList[x];
                        FinalList.Add(Item);
                    }
                    DeleteJoins(@object, source, batch, ManyToManyProperty, ManyToManyValueList);
                    int FinalListCount = FinalList.Count;
                    for (int x = 0; x < FinalListCount; ++x)
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
        private void CascadeManyToOneProperties(object @object, MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var ManyToOneProperty in ParentMappings.SelectMany(x => x.ManyToOneProperties).Where(x => x.Cascade))
            {
                var ManyToOneValue = ManyToOneProperty.GetValue(@object);
                if (ManyToOneValue != null)
                {
                    if (ManyToOneValue is IList ManyToOneValueList)
                    {
                        List<object> FinalList = new List<object>();
                        int ManyToManyValueListCount = ManyToOneValueList.Count;
                        for (int x = 0; x < ManyToManyValueListCount; ++x)
                        {
                            var Item = ManyToOneValueList[x];
                            FinalList.Add(Item);
                        }
                        int FinalListCount = FinalList.Count;
                        for (int x = 0; x < FinalListCount; ++x)
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
        private void CascadeMapProperties(object @object, MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen, IEnumerable<Inflatable.Interfaces.IMapping> ParentMappings)
        {
            foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties).Where(x => x.Cascade))
            {
                Delete(MapProperty.GetValue(@object), source, batch, objectsSeen);
            }
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        private void Delete(object @object, MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen)
        {
            if (@object == null || WasObjectSeen(@object, objectsSeen, source))
                return;
            objectsSeen.Add(@object);
            DeleteCascade(@object, source, batch, objectsSeen);
            var Generator = QueryProviderManager.CreateGenerator(@object.GetType(), source);
            var Queries = Generator.GenerateQueries(QueryType.Delete, @object);
            int QueriesLength = Queries.Length;
            for (int x = 0; x < QueriesLength; x++)
            {
                var TempQuery = Queries[x];
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
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
        private void DeleteCascade(object @object, MappingSource source, SQLHelper.SQLHelper batch, IList<object> objectsSeen)
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
        private void DeleteJoins(object @object, MappingSource source, SQLHelper.SQLHelper batch, IManyToManyProperty ManyToManyProperty, IList ManyToManyValueList)
        {
            if (ManyToManyProperty.DatabaseJoinsCascade)
                return;
            ManyToManyValueList.Clear();
            var LinksGenerator = QueryProviderManager.CreateGenerator(ManyToManyProperty.ParentMapping.ObjectType, source);
            var TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsDelete, @object, ManyToManyProperty);
            int TempQueriesLength = TempQueries.Length;
            for (int x = 0; x < TempQueriesLength; ++x)
            {
                var TempQuery = TempQueries[x];
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
        }
    }
}