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
using BigBook.Caching.Interfaces;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using SQLHelper.HelperClasses.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inflatable.Sessions
{
    /// <summary>
    /// Class for an individual session
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="schemaManager">The schema manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="aopManager">The aop manager.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// cacheManager or aopManager or mappingManager or schemaManager or queryProviderManager
        /// </exception>
        public Session(MappingManager mappingManager,
            SchemaManager schemaManager,
            QueryProviderManager queryProviderManager,
            Aspectus.Aspectus aopManager,
            BigBook.Caching.Manager cacheManager)
        {
            cacheManager = cacheManager ?? throw new System.ArgumentNullException(nameof(cacheManager));
            Cache = cacheManager.Cache();
            AOPManager = aopManager ?? throw new System.ArgumentNullException(nameof(aopManager));
            MappingManager = mappingManager ?? throw new System.ArgumentNullException(nameof(mappingManager));
            SchemaManager = schemaManager ?? throw new System.ArgumentNullException(nameof(schemaManager));
            QueryProviderManager = queryProviderManager ?? throw new System.ArgumentNullException(nameof(queryProviderManager));
        }

        /// <summary>
        /// Gets the aop manager.
        /// </summary>
        /// <value>The aop manager.</value>
        private Aspectus.Aspectus AOPManager;

        /// <summary>
        /// The mapping manager
        /// </summary>
        private MappingManager MappingManager;

        /// <summary>
        /// The query provider manager
        /// </summary>
        private QueryProviderManager QueryProviderManager;

        /// <summary>
        /// The schema manager
        /// </summary>
        private SchemaManager SchemaManager;

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        private ICache Cache { get; }

        /// <summary>
        /// Returns all items that match the criteria
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>All items that match the criteria</returns>
        public async Task<IEnumerable<TObject>> AllAsync<TObject>(params IParameter[] parameters)
            where TObject : class
        {
            parameters = parameters ?? new IParameter[0];
            string KeyName = typeof(TObject).GetName() + "_All_" + parameters.ToString(x => x.ToString(), "_");
            parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                return GetListCached<TObject>(KeyName);
            }
            var ReturnValue = new List<Dynamo>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var Query = Mapping.Queries[QueryType.All];
                    Batch.AddQuery((Command, ResultList, Result) =>
                    {
                        int ResultListCount = ResultList.Count;
                        for (int x = 0; x < ResultListCount; ++x)
                        {
                            CopyOrAdd(Result, IDProperties, ResultList[x]);
                        }
                    },
                    ReturnValue,
                    Query.QueryString,
                    Query.DatabaseCommandType,
                    parameters);
                }
                await Batch.RemoveDuplicateCommands().ExecuteAsync();
            }
            Cache.Add(KeyName, ReturnValue, new string[] { typeof(TObject).GetName() });
            return ConvertValues<TObject>(ReturnValue);
        }

        /// <summary>
        /// Returns the first item that match the criteria
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The first item that match the criteria</returns>
        public async Task<TObject> AnyAsync<TObject>(params IParameter[] parameters)
            where TObject : class
        {
            parameters = parameters ?? new IParameter[0];
            string KeyName = typeof(TObject).GetName() + "_Any_" + parameters.ToString(x => x.ToString(), "_");
            parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                return GetCached<TObject>(KeyName);
            }
            var ReturnValue = new List<Dynamo>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var Query = Mapping.Queries[QueryType.Any];
                    Batch.AddQuery((Command, ResultList, Result) =>
                    {
                        int ResultListCount = ResultList.Count;
                        for (int x = 0; x < ResultListCount; ++x)
                        {
                            CopyOrAdd(Result, IDProperties, ResultList[x]);
                        }
                    },
                    ReturnValue,
                    Query.QueryString,
                    Query.DatabaseCommandType,
                    parameters);
                }
                await Batch.RemoveDuplicateCommands().ExecuteAsync();
            }
            var FirstItem = ReturnValue.FirstOrDefault();
            if (FirstItem != null)
            {
                Cache.Add(KeyName, FirstItem, new string[] { typeof(TObject).GetName() });
            }
            return ConvertValue<TObject>(FirstItem);
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectToDelete">The object to delete.</param>
        /// <returns>The number of rows that were deleted across all sources.</returns>
        public async Task<int> DeleteAsync<TObject>(TObject objectToDelete)
            where TObject : class
        {
            if (objectToDelete == null)
                return 0;
            Cache.RemoveByTag(typeof(TObject).GetName());
            var ReturnValue = 0;
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var Query = Mapping.Queries[QueryType.Delete];
                    var IDParameters = IDProperties.ForEach(x => x.GetAsParameter(objectToDelete)).ToArray();
                    Batch.AddQuery(Query.QueryString, Query.DatabaseCommandType, IDParameters);
                }
                ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();
            }
            return ReturnValue;
        }

        /// <summary>
        /// Inserts the specified object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToInsert">The objects to insert.</param>
        /// <returns>The objects inserted.</returns>
        public async Task<TObject[]> InsertAsync<TObject>(params TObject[] objectsToInsert)
            where TObject : class
        {
            objectsToInsert = (objectsToInsert ?? new TObject[0]).Where(x => x != null).ToArray();
            if (objectsToInsert.Length == 0)
                return objectsToInsert;
            Cache.RemoveByTag(typeof(TObject).GetName());
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var ReferenceProperties = ParentMappings.SelectMany(x => x.ReferenceProperties);
                    var Query = Mapping.Queries[QueryType.Insert];
                    for (int x = 0; x < objectsToInsert.Length; ++x)
                    {
                        var Parameters = IDProperties.ForEach(y => y.GetAsParameter(objectsToInsert[x])).ToList();
                        Parameters.AddRange(ReferenceProperties.ForEach(y => y.GetAsParameter(objectsToInsert[x])));
                        var IDProperty = IDProperties.FirstOrDefault(y => y.AutoIncrement);
                        var ReturnedID = Batch.AddQuery((Command, ResultList, InsertObject) =>
                                                        {
                                                            if (IDProperty != null && IDProperty.AutoIncrement)
                                                            {
                                                                IDProperty.SetValue(InsertObject, IDProperty.GetValue((Dynamo)ResultList[0]));
                                                            }
                                                        },
                                                        objectsToInsert[x],
                                                        Query.QueryString,
                                                        Query.DatabaseCommandType,
                                                        Parameters.ToArray());
                        Query = Mapping.Queries[QueryType.InsertBulk];
                    }
                }
                await Batch.RemoveDuplicateCommands().ExecuteAsync();
            }
            return objectsToInsert;
        }

        /// <summary>
        /// Updates the specified object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToUpdate">The objects to update.</param>
        /// <returns>Number of rows updated.</returns>
        public async Task<int> UpdateAsync<TObject>(params TObject[] objectsToUpdate)
            where TObject : class
        {
            objectsToUpdate = (objectsToUpdate ?? new TObject[0]).Where(x => x != null).ToArray();
            if (objectsToUpdate.Length == 0)
                return 0;
            Cache.RemoveByTag(typeof(TObject).GetName());
            int ReturnValue = 0;
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var ReferenceProperties = ParentMappings.SelectMany(x => x.ReferenceProperties);
                    var Query = Mapping.Queries[QueryType.Update];
                    for (int x = 0; x < objectsToUpdate.Length; ++x)
                    {
                        var Parameters = IDProperties.ForEach(y => y.GetAsParameter(objectsToUpdate[x])).ToList();
                        Parameters.AddRange(ReferenceProperties.ForEach(y => y.GetAsParameter(objectsToUpdate[x])));
                        Batch.AddQuery(Query.QueryString, Query.DatabaseCommandType, Parameters.ToArray());
                    }
                }
                ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();
            }
            return ReturnValue;
        }

        /// <summary>
        /// Copies the item to the result list or adds it.
        /// </summary>
        /// <param name="returnValue">The return list.</param>
        /// <param name="idProperties">The identifier properties.</param>
        /// <param name="item">The item.</param>
        private static void CopyOrAdd(List<Dynamo> returnValue, IEnumerable<IIDProperty> idProperties, Dynamo item)
        {
            if (item == null || returnValue == null)
                return;

            var Value = returnValue.FirstOrDefault(x => idProperties.All(y => y.GetValue(x).Equals(y.GetValue(item))));
            if (Value == null)
                returnValue.Add(item);
            else
                item.CopyTo(Value);
        }

        /// <summary>
        /// Converts the value to the specified type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="ReturnValue">The return value.</param>
        /// <returns>The converted values.</returns>
        private TObject ConvertValue<TObject>(Dynamo ReturnValue)
            where TObject : class
        {
            if (ReturnValue == null)
                return default(TObject);
            var Value = ReturnValue.To<TObject>();
            ((IORMObject)Value).Session0 = this;
            return Value;
        }

        /// <summary>
        /// Converts the values to the specified type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="ReturnValue">The return value.</param>
        /// <returns>The converted values.</returns>
        private IEnumerable<TObject> ConvertValues<TObject>(List<Dynamo> ReturnValue)
                    where TObject : class
        {
            ReturnValue = ReturnValue ?? new List<Dynamo>();
            return ReturnValue.ForEachParallel(x => ConvertValue<TObject>(x));
        }

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="keyName">Name of the key.</param>
        /// <returns>The cached value</returns>
        private TObject GetCached<TObject>(string keyName)
            where TObject : class
        {
            var ReturnValue = (Dynamo)Cache[keyName];
            return ConvertValue<TObject>(ReturnValue);
        }

        /// <summary>
        /// Gets the cached items as a list.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="keyName">Name of the key.</param>
        /// <returns>The resulting list.</returns>
        private IEnumerable<TObject> GetListCached<TObject>(string keyName)
                    where TObject : class
        {
            var ReturnValue = (List<Dynamo>)Cache[keyName];
            return ConvertValues<TObject>(ReturnValue);
        }
    }
}