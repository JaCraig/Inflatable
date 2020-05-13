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
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Schema;
using Inflatable.Sessions.Commands;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using SQLHelperDB.HelperClasses;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflatable.Sessions
{
    /// <summary>
    /// Class for an individual session
    /// </summary>
    /// <seealso cref="ISession"/>
    public class Session : ISession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="schemaManager">The schema manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="dynamoFactory">The dynamo factory.</param>
        /// <exception cref="ArgumentNullException">
        /// cacheManager or aopManager or mappingManager or schemaManager or queryProviderManager
        /// </exception>
        public Session(MappingManager mappingManager,
            SchemaManager schemaManager,
            QueryProviderManager queryProviderManager,
            ILogger logger,
            BigBook.Caching.Manager cacheManager,
            DynamoFactory dynamoFactory,
            ObjectPool<StringBuilder> objectPool)
        {
            if (mappingManager is null)
                throw new ArgumentNullException(nameof(mappingManager));
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            Commands = new List<Commands.Interfaces.ICommand>();
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Cache = cacheManager?.Cache();
            DynamoFactory = dynamoFactory;
            ReadSources = mappingManager.ReadSources;
            WriteSources = mappingManager.WriteSources;
            ObjectPool = objectPool;
        }

        /// <summary>
        /// Gets the dynamo factory.
        /// </summary>
        /// <value>The dynamo factory.</value>
        public DynamoFactory DynamoFactory { get; }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        private ICache Cache { get; }

        /// <summary>
        /// Gets or sets the commands.
        /// </summary>
        /// <value>The commands.</value>
        private List<Commands.Interfaces.ICommand> Commands { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// The query provider manager
        /// </summary>
        private QueryProviderManager QueryProviderManager { get; }

        /// <summary>
        /// Gets the read only sources.
        /// </summary>
        /// <value>The read only sources.</value>
        private IMappingSource[] ReadSources { get; }

        /// <summary>
        /// Gets the write sources.
        /// </summary>
        /// <value>The write sources.</value>
        private IMappingSource[] WriteSources { get; }

        /// <summary>
        /// Adds the objects for deletion.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns>This.</returns>
        public ISession Delete<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            Commands.Add(new DeleteCommand(QueryProviderManager, Cache, objectsToDelete));
            return this;
        }

        /// <summary>
        /// Executes all queued commands.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public int Execute()
        {
            return Task.Run(async () => await ExecuteAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes all queued commands.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteAsync()
        {
            var Result = 0;
            RemoveDuplicateCommands();
            for (var i = 0; i < WriteSources.Length; i++)
            {
                for (int x = 0, CommandsCount = Commands.Count; x < CommandsCount; ++x)
                {
                    Result += await Commands[x].ExecuteAsync(WriteSources[i], DynamoFactory).ConfigureAwait(false);
                }
            }
            Commands.Clear();
            return Result;
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
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<TObject>> ExecuteAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
            where TObject : class
        {
            parameters ??= Array.Empty<IParameter>();
            var Source = Array.Find(ReadSources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }
            var ObjectType = Source.GetChildMappings(typeof(TObject)).First().ObjectType;
            var Parameters = ConvertParameters(parameters);
            var KeyName = command + "_" + connection;
            Parameters.ForEach(x => KeyName = x.AddParameter(KeyName));
            if (Cache.TryGetValue(KeyName, out var ReturnValue) && ReturnValue is Dynamo[] ReturnValueDynamos)
            {
                return Convert<TObject>(ObjectType, ReturnValueDynamos);
            }

            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
            Batch.AddQuery(type, command, Parameters);
            try
            {
                var Results = (await Batch.ExecuteAsync().ConfigureAwait(false)).SelectMany(x => x.Cast<Dynamo>()).ToArray();

                var Tags = new List<string>();
                GetTags(Tags, string.Empty, ObjectType);
                Cache.Add(KeyName, Results, Tags);
                return Convert<TObject>(ObjectType, Results);
            }
            catch
            {
                Logger.Debug("Failed on query: " + Batch);
                throw;
            }
        }

        /// <summary>
        /// Executes the specified command and returns items of a specific type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries to run.</param>
        /// <returns>The resulting data</returns>
        public async Task<IEnumerable<dynamic>> ExecuteAsync<TObject>(IDictionary<IMappingSource, QueryData<TObject>> queries)
            where TObject : class
        {
            if (queries is null)
                return Array.Empty<TObject>();
            if (queries.Any(x => x.Value.SelectValues.Count > 0))
            {
                return await GetSubView(queries).ConfigureAwait(false);
            }
            var ObjectType = typeof(TObject);
            var TempReadSources = queries.Keys.ToArray();
            var SessionQueryInfo = GetSessionQueryInfo(ObjectType, TempReadSources);
            //TODO: Reuse the query info so I'm not making redundant calls.

            var KeyName = GetIDListCacheKey(queries.Values);
            var IDList = GetCachedIDList(KeyName, Cache);
            IDList = await GetIDList(IDList, KeyName, queries).ConfigureAwait(false);
            var MissingCachedItems = GetMissedCachedItems<TObject>(IDList, Cache, TempReadSources);
            await FillCache<TObject>(MissingCachedItems, TempReadSources).ConfigureAwait(false);
            return GetCachedItems<TObject>(IDList, Cache, SessionQueryInfo.Values.Select(x => x.AssociatedMapping).FirstOrDefault(x => !(x is null))) ?? Array.Empty<TObject>();
        }

        /// <summary>
        /// Executes the specified command and returns the count.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries to run.</param>
        /// <returns>The resulting data</returns>
        public async Task<int> ExecuteCountAsync<TObject>(IDictionary<IMappingSource, QueryData<TObject>> queries)
            where TObject : class
        {
            var Results = new List<QueryResults>();
            var FirstRun = true;
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator is null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            return (int)(Results?.FirstOrDefault(x => (int)(x.Values.FirstOrDefault()?["Count"] ?? 0) > 0)?.Values[0]["Count"] ?? 0);
        }

        /// <summary>
        /// Executes the specified command and returns items of a specific type.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<dynamic>> ExecuteDynamicAsync(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters ??= Array.Empty<IParameter>();
            var Parameters = ConvertParameters(parameters);
            var Source = Array.Find(ReadSources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }

            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
            Batch.AddQuery(type, command, Parameters);
            try
            {
                return (await Batch.ExecuteAsync().ConfigureAwait(false))[0];
            }
            catch
            {
                Logger.Debug("Failed on query: " + Batch.ToString());
                throw;
            }
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
        /// <exception cref="ArgumentException"></exception>
        public Task<TObject> ExecuteScalarAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters ??= Array.Empty<IParameter>();
            var Parameters = ConvertParameters(parameters);
            var Source = Array.Find(ReadSources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }

            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);

            Batch.AddQuery(type, command, Parameters);
            try
            {
                return Batch.ExecuteScalarAsync<TObject>();
            }
            catch
            {
                Logger.Debug("Failed on query: " + Batch.ToString());
                throw;
            }
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The appropriate property value</returns>
        public IList<TData> LoadProperties<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class
        {
            return Task.Run(async () => await LoadPropertiesAsync<TObject, TData>(objectToLoadProperty, propertyName).ConfigureAwait(false)).GetAwaiter().GetResult();
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
            Type ObjectType = typeof(TObject);
            var ParentMapping = GetParentMapping(ObjectType, ReadSources);
            if (ParentMapping is null)
                return Array.Empty<TData>().ToObservableList(x => x);
            var ReadOnlySources = ReadSources.Where(x => x.GetChildMappings(typeof(TData)).Any()).ToArray();
            foreach (var Source in ReadOnlySources)
            {
                var TempProperty = FindProperty<TObject, TData>(Source, propertyName);
                if (!(TempProperty?.LoadPropertyQuery is null))
                {
                    var Parameters = ParentMapping.IDProperties.Select(x => x.GetColumnInfo()[0].GetAsParameter(objectToLoadProperty)).ToArray();
                    return (await ExecuteAsync<TData>(TempProperty.LoadPropertyQuery.QueryString, TempProperty.LoadPropertyQuery.DatabaseCommandType, Source.Source.Name, Parameters!).ConfigureAwait(false)).ToObservableList(x => x);
                }
            }
            var IDList = await GetIDList<TObject, TData>(objectToLoadProperty, propertyName).ConfigureAwait(false);
            var MissingCachedItems = GetMissedCachedItems<TData>(IDList, Cache, ReadOnlySources);
            await FillCache<TData>(MissingCachedItems, ReadOnlySources).ConfigureAwait(false);
            return (GetCachedItems<TData>(IDList, Cache, ParentMapping) ?? Array.Empty<TData>()).ToObservableList(x => x);
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The appropriate property value</returns>
        public TData LoadProperty<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class => LoadProperties<TObject, TData>(objectToLoadProperty, propertyName).FirstOrDefault();

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
            where TData : class => (await LoadPropertiesAsync<TObject, TData>(objectToLoadProperty, propertyName).ConfigureAwait(false)).FirstOrDefault();

        /// <summary>
        /// Adds the specified objects to save.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToSave">The objects to save.</param>
        /// <returns>This</returns>
        public ISession Save<TObject>(params TObject[] objectsToSave)
            where TObject : class
        {
            Commands.Add(new SaveCommand(QueryProviderManager, Cache, objectsToSave));
            return this;
        }

        /// <summary>
        /// Caches the results.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="results">The results.</param>
        /// <param name="cache">The cache.</param>
        private static void CacheResults(string keyName, List<QueryResults> results, ICache cache)
        {
            var Tags = new List<string>();
            foreach (var Result in results)
            {
                GetTags(Tags, string.Empty, Result.Query.ReturnType);
            }
            cache.Add(keyName, results, Tags);
        }

        /// <summary>
        /// Converts the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static IParameter[] ConvertParameters(object[] parameters)
        {
            if (parameters is null || parameters.Length == 0)
                return Array.Empty<IParameter>();
            var Parameters = new IParameter[parameters.Length];
            for (int x = 0, parametersLength = parameters.Length; x < parametersLength; x++)
            {
                var CurrentParameter = parameters[x];
                if (CurrentParameter is IParameter TempQueryParameter)
                {
                    Parameters[x] = TempQueryParameter;
                }
                else if (CurrentParameter is null)
                {
                    Parameters[x] = new Parameter<object>(x.ToString(CultureInfo.InvariantCulture), null!);
                }
                else if (CurrentParameter is string TempParameter)
                {
                    Parameters[x] = new StringParameter(x.ToString(CultureInfo.InvariantCulture), TempParameter);
                }
                else
                {
                    Parameters[x] = new Parameter<object>(x.ToString(CultureInfo.InvariantCulture), CurrentParameter);
                }
            }

            return Parameters;
        }

        /// <summary>
        /// Finds the property.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property</returns>
        private static IClassProperty FindProperty<TObject, TData>(IMappingSource source, string propertyName)
            where TObject : class
            where TData : class
        {
            var ParentMappings = source.GetChildMappings(typeof(TObject)).SelectMany(x => source.GetParentMapping(x.ObjectType)).Distinct();
            IClassProperty Property = ParentMappings.SelectMany(x => x.ManyToManyProperties).FirstOrDefault(x => x.Name == propertyName);
            if (!(Property is null))
            {
                return Property;
            }

            Property = ParentMappings.SelectMany(x => x.ManyToOneProperties).FirstOrDefault(x => x.Name == propertyName);
            return Property ?? ParentMappings.SelectMany(x => x.MapProperties).FirstOrDefault(x => x.Name == propertyName);
        }

        /// <summary>
        /// Gets the cached identifier list.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="cache">The cache.</param>
        /// <returns>The cached ID list.</returns>
        private static Dynamo[] GetCachedIDList(string keyName, ICache cache)
        {
            if (cache.TryGetValue(keyName, out var Value) && Value is Dynamo[] ReturnValue)
                return ReturnValue;
            return Array.Empty<Dynamo>();
        }

        /// <summary>
        /// Gets the cached values.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        private static IEnumerable<TObject> GetCachedValues<TObject>(string keyName, ICache cache)
                    where TObject : class
        {
            if (!cache.TryGetValue(keyName, out var ReturnValue) || !(ReturnValue is List<QueryResults> QueryResults))
                return Array.Empty<TObject>();
            return QueryResults.SelectMany(x => x.ConvertValues<TObject>()) ?? Array.Empty<TObject>();
        }

        /// <summary>
        /// Gets the name of the cache identifier.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string GetCacheIDName<TObject>(IMapping parentMapping, Dynamo value)
        {
            return GetCacheIDName(typeof(TObject), parentMapping, value);
        }

        /// <summary>
        /// Gets the name of the cache identifier.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string GetCacheIDName(Type objectType, IMapping parentMapping, Dynamo value)
        {
            if (parentMapping is null)
                return string.Empty;
            var IDNames = parentMapping.IDProperties.OrderBy(x => x.Name).ToString(x => x.Name + "_" + x.GetColumnInfo()[0].GetValue(value)?.ToString() ?? string.Empty, "_");
            return $"{objectType.Name}_{IDNames}";
        }

        /// <summary>
        /// Gets the identifier list cache key.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries.</param>
        /// <returns>The ID list cache key</returns>
        private static string GetIDListCacheKey<TObject>(ICollection<QueryData<TObject>>? queries)
                where TObject : class
        {
            var ReturnValue = queries?.ToString(x => x + "_" + x.Source.Source.Name, "\n") ?? string.Empty;
            foreach (var Parameter in queries?.SelectMany(x => x.Parameters)?.Distinct() ?? Array.Empty<IParameter>())
            {
                ReturnValue = Parameter.AddParameter(ReturnValue);
            }
            return ReturnValue;
        }

        /// <summary>
        /// Gets the missed cached items.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="idList">The identifier list.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="sources">The sources.</param>
        /// <returns></returns>
        private static Dynamo[] GetMissedCachedItems<TObject>(Dynamo[] idList, ICache cache, IMappingSource[] sources)
        {
            var ObjectType = typeof(TObject);
            foreach (var Source in sources)
            {
                var ParentMapping = GetParentMapping(ObjectType, Source);
                if (ParentMapping is null)
                    continue;
                idList = idList.Where(x => !cache.GetByTag(GetCacheIDName(ObjectType, ParentMapping, x)).Any()).ToArray();
            }
            return idList;
        }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="source">The source.</param>
        /// <returns>The parent mapping</returns>
        private static IMapping? GetParentMapping(Type objectType, IMappingSource source)
        {
            return source.GetChildMappings(objectType)
                        .SelectMany(x => source.GetParentMapping(x.ObjectType))
                        .Distinct()
                        .FirstOrDefault(x => x.IDProperties.Count > 0);
        }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>The parent mapping</returns>
        private static IMapping? GetParentMapping(Type objectType, IMappingSource[] sources)
        {
            return sources
                .SelectMany(x => x.GetChildMappings(objectType).SelectMany(y => x.GetParentMapping(y.ObjectType)))
                .Distinct()
                .FirstOrDefault(x => x.IDProperties.Count > 0);
        }

        /// <summary>
        /// Gets the session query information.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>The session query info for each source.</returns>
        private static Dictionary<IMappingSource, SessionQueryInfo> GetSessionQueryInfo(Type objectType, IMappingSource[] sources)
        {
            var ReturnValue = new Dictionary<IMappingSource, SessionQueryInfo>();
            for (int x = 0; x < sources.Length; ++x)
            {
                var ChildMappings = sources[x].GetChildMappings(objectType).ToArray();
                var ParentMappings = ChildMappings.SelectMany(y => sources[x].GetParentMapping(y.ObjectType)).Distinct().ToArray();
                ReturnValue.Add(sources[x], new SessionQueryInfo(sources[x], ChildMappings, ParentMappings));
            }
            return ReturnValue;
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="tagList">The tag list.</param>
        /// <param name="iDValue">The id value.</param>
        /// <param name="type">The type.</param>
        private static void GetTags(List<string> tagList, string? iDValue, Type type)
        {
            if (type is null)
                return;
            var TempType = type;
            while (TempType != typeof(object))
            {
                tagList.AddIfUnique(TempType.Name + iDValue);
                TempType = TempType.BaseType;
                if (TempType is null)
                    break;
            }
            var Interfaces = type.GetInterfaces();
            for (int i = 0; i < Interfaces.Length; i++)
            {
                tagList.AddIfUnique(Interfaces[i].Name + iDValue);
            }
        }

        /// <summary>
        /// Converts the specified object type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="dynamos">The dynamos.</param>
        /// <returns>The converted values.</returns>
        private TObject[] Convert<TObject>(Type objectType, Dynamo[] dynamos)
                    where TObject : class
        {
            var ReturnValues = new TObject[dynamos.Length];
            for (int x = 0; x < dynamos.Length; ++x)
            {
                var Value = dynamos[x].To(objectType) as TObject;
                if (Value is IORMObject oRMObject)
                    oRMObject.Session0 = this;
                ReturnValues[x] = Value!;
            }
            return ReturnValues;
        }

        /// <summary>
        /// Copies the results.
        /// </summary>
        /// <param name="Results">The results.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Queries">The queries.</param>
        /// <param name="ResultLists">The result lists.</param>
        /// <param name="firstRun">if set to <c>true</c> [first run].</param>
        private void CopyResults(List<QueryResults> Results, IMappingSource Source, IQuery[] Queries, List<List<dynamic>>? ResultLists, bool firstRun)
        {
            if (ResultLists is null)
                return;
            for (int x = 0, ResultListsCount = ResultLists.Count; x < ResultListsCount; ++x)
            {
                if (Queries.Length <= x)
                    continue;
                var IDProperties = Source.GetParentMapping(Queries[x].ReturnType).SelectMany(y => y.IDProperties);
                var TempQuery = new QueryResults(Queries[x], ResultLists[x].Cast<Dynamo>(), this);
                var Result = Results.Find(y => y.CanCopy(TempQuery, IDProperties));
                if (Result is null && firstRun)
                {
                    Results.Add(TempQuery);
                }
                else if (firstRun)
                {
                    Result?.Add(TempQuery);
                }
                else
                {
                    Result?.Copy(TempQuery, IDProperties);
                }
            }
        }

        /// <summary>
        /// Fills the cache.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="missingCachedItems">The missing cached items.</param>
        /// <param name="mappingSources">The mapping sources.</param>
        /// <returns></returns>
        private async Task FillCache<TObject>(Dynamo[] missingCachedItems, IMappingSource[] mappingSources)
                                            where TObject : class
        {
            if (missingCachedItems is null || missingCachedItems.Length == 0)
                return;
            var results = new List<QueryResults>();
            var firstRun = true;
            //Run queries
            for (int i = 0; i < mappingSources.Length; ++i)
            {
                var Source = mappingSources[i];
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
                if (Generator is null)
                    continue;
                var ResultingQueries = Generator.GenerateQueries(QueryType.LoadData, missingCachedItems);
                var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
                for (int x = 0, ResultingQueriesLength = ResultingQueries.Length; x < ResultingQueriesLength; x++)
                {
                    var ResultingQuery = ResultingQueries[x];
                    Batch.AddQuery(ResultingQuery.DatabaseCommandType, ResultingQuery.QueryString, ResultingQuery.Parameters!);
                }

                List<List<dynamic>>? Result = null;
                try
                {
                    Result = await Batch.ExecuteAsync().ConfigureAwait(false);
                }
                catch
                {
                    Logger.Debug("Failed on query: " + Batch);
                    throw;
                }
                CopyResults(results, Source, ResultingQueries, Result, firstRun);
                firstRun = false;
            }
            //Fill cache
            foreach (var QueryResult in results)
            {
                foreach (var Result in QueryResult.Values)
                {
                    List<string> TagList = new List<string>();
                    string Key = string.Empty;
                    for (int i = 0; i < mappingSources.Length; ++i)
                    {
                        var Source = mappingSources[i];
                        var IDValue = "_" + GetParentMapping(QueryResult.Query.ReturnType, Source)?
                            .IDProperties
                            .OrderBy(x => x.Name)
                            .ToString(x => x.Name + "_" + x.GetColumnInfo()[0].GetValue(Result)?.ToString() ?? string.Empty, "_");
                        GetTags(TagList, IDValue, QueryResult.Query.ReturnType);
                    }
                    Cache.Add(Guid.NewGuid().ToString(), new CachedResult(Result, QueryResult.Query.ReturnType), TagList);
                }
            }
        }

        /// <summary>
        /// Generates the query asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="results">The results.</param>
        /// <param name="firstRun">if set to <c>true</c> [first run].</param>
        /// <param name="source">The source.</param>
        /// <returns>The results of the query on the source</returns>
        private async Task GenerateQueryAsync<TObject>(List<QueryResults> results, bool firstRun, KeyValuePair<IMappingSource, QueryData<TObject>> source)
            where TObject : class
        {
            var Generator = QueryProviderManager.CreateGenerator<TObject>(source.Key);
            if (Generator is null)
                return;
            var ResultingQueries = Generator.GenerateQueries(source.Value);
            var Batch = QueryProviderManager.CreateBatch(source.Key.Source, DynamoFactory);
            for (int x = 0, ResultingQueriesLength = ResultingQueries.Length; x < ResultingQueriesLength; x++)
            {
                var ResultingQuery = ResultingQueries[x];
                Batch.AddQuery(ResultingQuery.DatabaseCommandType, ResultingQuery.QueryString, ResultingQuery.Parameters!);
            }

            List<List<dynamic>>? Result;
            try
            {
                Result = await Batch.ExecuteAsync().ConfigureAwait(false);
            }
            catch
            {
                Logger.Debug("Failed on query: " + Batch);
                throw;
            }
            CopyResults(results, source.Key, ResultingQueries, Result, firstRun);
        }

        /// <summary>
        /// Gets the cached items.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="idList">The identifier list.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <returns></returns>
        private TObject[] GetCachedItems<TObject>(Dynamo[] idList, ICache cache, IMapping parentMapping)
                    where TObject : class
        {
            TObject[] ReturnValue = new TObject[idList.Length];
            for (int x = 0; x < idList.Length; ++x)
            {
                var ID = idList[x];
                var Key = GetCacheIDName<TObject>(parentMapping, ID);
                var Value = cache.GetByTag(Key).FirstOrDefault();
                if (Value is CachedResult cachedResult)
                {
                    var TempVal = cachedResult.Value.To(cachedResult.ObjectType);
                    ((IORMObject)TempVal).Session0 = this;
                    ReturnValue[x] = (TempVal as TObject)!;
                }
            }
            return ReturnValue;
        }

        /// <summary>
        /// Gets the identifier list.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private async Task<Dynamo[]> GetIDList<TObject, TData>(TObject objectToLoadProperty, string propertyName)
                    where TObject : class
                    where TData : class
        {
            var Results = new List<QueryResults>();
            var Tags = new List<string>();
            foreach (var Source in ReadSources)
            {
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
                if (Generator is null)
                    continue;
                var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
                var Property = FindProperty<TObject, TData>(Source, propertyName);
                var Queries = Generator.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property);
                for (int x = 0, QueriesLength = Queries.Length; x < QueriesLength; x++)
                {
                    var TempQuery = Queries[x];
                    Batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
                }

                List<List<dynamic>>? ResultLists = null;

                try
                {
                    ResultLists = await Batch.ExecuteAsync().ConfigureAwait(false);
                }
                catch
                {
                    Logger.Debug("Failed on query: " + Batch);
                    throw;
                }

                CopyResults(Results, Source, Queries, ResultLists, true);
            }
            return Results.SelectMany(x => x.Values).ToArray();
        }

        /// <summary>
        /// Gets the identifier list.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="idList">The identifier list.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="queries">The queries.</param>
        /// <returns></returns>
        private async Task<Dynamo[]> GetIDList<TObject>(Dynamo[] idList, string keyName, IDictionary<IMappingSource, QueryData<TObject>> queries)
                    where TObject : class
        {
            if (idList.Length > 0)
                return idList;
            var Results = new List<QueryResults>();
            var FirstRun = true;
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator is null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            var Tags = new List<string>();
            foreach (var Result in Results)
            {
                GetTags(Tags, string.Empty, Result.Query.ReturnType);
            }
            var ReturnValue = Results.SelectMany(x => x.Values).ToArray();
            Cache.Add(keyName, ReturnValue, Tags);
            return ReturnValue;
        }

        /// <summary>
        /// Gets the sub view.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries.</param>
        /// <returns></returns>
        private async Task<IEnumerable<dynamic>> GetSubView<TObject>(IDictionary<IMappingSource, QueryData<TObject>> queries)
                    where TObject : class
        {
            var KeyName = GetIDListCacheKey(queries.Values);
            var ObjectList = GetCachedValues<TObject>(KeyName, Cache);
            if (ObjectList.Any())
                return ObjectList;
            var Results = new List<QueryResults>();
            var FirstRun = true;

            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator is null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            CacheResults(KeyName, Results, Cache);
            return Results?.SelectMany(x => x.ConvertValues<TObject>())?.ToArray() ?? Array.Empty<TObject>();
        }

        /// <summary>
        /// Removes the duplicate commands.
        /// </summary>
        private void RemoveDuplicateCommands()
        {
            var CommandsCount = Commands.Count;
            for (var x = 0; x < CommandsCount; ++x)
            {
                for (var y = x + 1; y < CommandsCount; ++y)
                {
                    if (Commands[x].Merge(Commands[y]))
                    {
                        Commands.RemoveAt(y);
                        --y;
                        --CommandsCount;
                    }
                }
            }
        }
    }
}