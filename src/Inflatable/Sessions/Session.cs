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
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        /// <exception cref="ArgumentNullException">
        /// cacheManager or aopManager or mappingManager or schemaManager or queryProviderManager
        /// </exception>
        public Session(MappingManager mappingManager,
            SchemaManager schemaManager,
            QueryProviderManager queryProviderManager,
            Aspectus.Aspectus aopManager,
            BigBook.Caching.Manager cacheManager)
        {
            cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            Cache = cacheManager.Cache();
            AOPManager = aopManager ?? throw new ArgumentNullException(nameof(aopManager));
            MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
            SchemaManager = schemaManager ?? throw new ArgumentNullException(nameof(schemaManager));
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
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
                await Batch.ExecuteAsync();
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
                await Batch.ExecuteAsync();
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
        /// <param name="objectsToDelete">The object to delete.</param>
        /// <returns>The number of rows that were deleted across all sources.</returns>
        public async Task<int> DeleteAsync<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            objectsToDelete = (objectsToDelete ?? new TObject[0]).Where(x => x != null).ToArray();
            if (objectsToDelete.Length == 0)
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
                    for (int x = 0; x < objectsToDelete.Length; ++x)
                    {
                        var IDParameters = IDProperties.ForEach(y => y.GetAsParameter(objectsToDelete[x])).ToArray();
                        Batch.AddQuery(Query.QueryString, Query.DatabaseCommandType, IDParameters);
                    }
                }
                ReturnValue += await Batch.RemoveDuplicateCommands().ExecuteScalarAsync<int>();
            }
            return ReturnValue;
        }

        /// <summary>
        /// Executes the specified command and returns items of a specific type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<IEnumerable<TObject>> ExecuteAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
            where TObject : class
        {
            parameters = parameters ?? new IParameter[0];
            List<IParameter> Parameters = ConvertParameters(parameters);
            string KeyName = command + "_" + connection;
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                return GetListCached<TObject>(KeyName);
            }
            var Source = MappingManager.Sources.FirstOrDefault(x => x.Source.Name == connection);
            if (Source == null)
                throw new ArgumentException($"Source not found {connection}");
            var IDProperties = Source.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source);
            Batch.AddQuery((Command, ResultList, Result) =>
            {
                int ResultListCount = ResultList.Count;
                for (int x = 0; x < ResultListCount; ++x)
                {
                    CopyOrAdd(Result, IDProperties, ResultList[x]);
                }
            },
            ReturnValue,
            command,
            type,
            Parameters.ToArray());
            await Batch.ExecuteAsync();

            Cache.Add(KeyName, ReturnValue, new string[] { typeof(TObject).GetName() });
            return ConvertValues<TObject>(ReturnValue);
        }

        /// <summary>
        /// Executes the specified command and returns items of a specific type.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<IEnumerable<dynamic>> ExecuteAsync(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters = parameters ?? new IParameter[0];
            List<IParameter> Parameters = ConvertParameters(parameters);
            var Source = MappingManager.Sources.FirstOrDefault(x => x.Source.Name == connection);
            if (Source == null)
                throw new ArgumentException($"Source not found {connection}");
            var Batch = QueryProviderManager.CreateBatch(Source.Source);
            Batch.AddQuery(command,
            type,
            Parameters.ToArray());
            return (await Batch.ExecuteAsync())[0];
        }

        /// <summary>
        /// Executes the specified command and returns items of a specific type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries to run.</param>
        /// <returns>The resulting data</returns>
        public async Task<IEnumerable<dynamic>> ExecuteAsync<TObject>(IDictionary<MappingSource, QueryData<TObject>> queries)
            where TObject : class
        {
            string KeyName = queries.Values.ToString(x => x + "_" + x.Source.Source.Name, "\n");
            queries.Values
                .SelectMany(x => x.Parameters)
                .Distinct()
                .ForEach(x =>
                {
                    KeyName = x.AddParameter(KeyName);
                });
            if (Cache.ContainsKey(KeyName))
            {
                return GetListCached<TObject>(KeyName);
            }
            List<Dynamo> Results = new List<Dynamo>();
            bool FirstRun = true;
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                          .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator == null)
                                          .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            Cache.Add(KeyName, Results, new string[] { typeof(TObject).GetName() });
            return ConvertValues<TObject>(Results);
        }

        /// <summary>
        /// Executes the specified command and returns the first item of a specific type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<TObject> ExecuteScalarAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters = parameters ?? new IParameter[0];
            List<IParameter> Parameters = ConvertParameters(parameters);
            var Source = MappingManager.Sources.FirstOrDefault(x => x.Source.Name == connection);
            if (Source == null)
                throw new ArgumentException($"Source not found {connection}");
            var IDProperties = Source.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source);
            Batch.AddQuery((Command, ResultList, Result) =>
            {
                int ResultListCount = ResultList.Count;
                for (int x = 0; x < ResultListCount; ++x)
                {
                    CopyOrAdd(Result, IDProperties, ResultList[x]);
                }
            },
            ReturnValue,
            command,
            type,
            Parameters.ToArray());
            return await Batch.ExecuteScalarAsync<TObject>();
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
                await Batch.ExecuteAsync();
            }
            return objectsToInsert;
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The appropriate property value</returns>
        public async Task<IList<TData>> LoadPropertiesAsync<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class
        {
            var ReturnValue = new List<Dynamo>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead).OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TData));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var Property = Mapping.MapProperties.FirstOrDefault(x => x.Name == propertyName);
                    if (Property == null)
                        continue;
                    var Query = Property.LoadPropertyQuery;
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
                    IDProperties.ToArray(x => x.GetAsParameter(objectToLoadProperty)));
                }
                await Batch.ExecuteAsync();
            }
            return ConvertValues<TData>(ReturnValue).ToObservableList(x => x);
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The appropriate property value</returns>
        public async Task<TData> LoadPropertyAsync<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class
        {
            return (await LoadPropertiesAsync<TObject, TData>(objectToLoadProperty, propertyName)).FirstOrDefault();
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

        private static List<IParameter> ConvertParameters(object[] parameters)
        {
            var Parameters = new List<IParameter>();
            foreach (object CurrentParameter in parameters)
            {
                var TempQueryParameter = CurrentParameter as IParameter;
                var TempParameter = CurrentParameter as string;
                if (TempQueryParameter != null)
                    Parameters.Add(TempQueryParameter);
                else if (CurrentParameter == null)
                    Parameters.Add(new Parameter<object>(Parameters.Count().ToString(CultureInfo.InvariantCulture), null));
                else if (TempParameter != null)
                    Parameters.Add(new StringParameter(Parameters.Count().ToString(CultureInfo.InvariantCulture), TempParameter));
                else
                    Parameters.Add(new Parameter<object>(Parameters.Count().ToString(CultureInfo.InvariantCulture), CurrentParameter));
            }

            return Parameters;
        }

        private static void Copy(List<Dynamo> returnValue, IEnumerable<IIDProperty> idProperties, Dynamo item)
        {
            if (item == null
                || returnValue == null
                || idProperties.Count() == 0)
                return;
            var Value = returnValue.FirstOrDefault(x => idProperties.All(y => y.GetValue(x).Equals(y.GetValue(item))));
            if (Value == null)
                return;
            item.CopyTo(Value);
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
            if (idProperties.Count() == 0)
            {
                returnValue.Add(item);
                return;
            }
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
        private IList<TObject> ConvertValues<TObject>(List<Dynamo> ReturnValue)
                    where TObject : class
        {
            ReturnValue = ReturnValue ?? new List<Dynamo>();
            return ReturnValue.ForEachParallel(x => ConvertValue<TObject>(x)).ToList();
        }

        /// <summary>
        /// Generates the query asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="results">The results.</param>
        /// <param name="firstRun">if set to <c>true</c> [first run].</param>
        /// <param name="source">The source.</param>
        /// <returns>The results of the query on the source</returns>
        private async Task GenerateQueryAsync<TObject>(List<Dynamo> results, bool firstRun, KeyValuePair<MappingSource, QueryData<TObject>> source)
            where TObject : class
        {
            var Generator = QueryProviderManager.CreateGenerator<TObject>(source.Key);
            var ResultingQuery = Generator.GenerateQuery(source.Value);
            var IDProperties = source.Key.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var IndividualQueryResults = await ExecuteAsync(ResultingQuery.QueryString,
                ResultingQuery.DatabaseCommandType,
                source.Key.Source.Name,
                source.Value.Parameters.ToArray());
            foreach (var Item in IndividualQueryResults)
            {
                if (firstRun)
                {
                    CopyOrAdd(results, IDProperties, Item);
                }
                else
                {
                    Copy(results, IDProperties, Item);
                }
            }
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
        private IList<TObject> GetListCached<TObject>(string keyName)
                    where TObject : class
        {
            var ReturnValue = (List<Dynamo>)Cache[keyName];
            return ConvertValues<TObject>(ReturnValue);
        }
    }
}