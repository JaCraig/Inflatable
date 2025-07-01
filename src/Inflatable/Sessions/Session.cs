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
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using DragonHoard.InMemory;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Registration;
using Inflatable.Schema;
using Inflatable.Sessions.Commands;
using Inflatable.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ObjectCartographer;
using SQLHelperDB.HelperClasses;
using SQLHelperDB.HelperClasses.Interfaces;
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
    /// <seealso cref="ISession"/>
    public class Session : ISession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="aspectus">The aspectus.</param>
        /// <param name="dataMapper">The data mapper.</param>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="schemaManager">The schema manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="options">The options.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The Logger?.</param>
        /// <exception cref="ArgumentNullException">
        /// mappingManager or queryProviderManager or logger
        /// </exception>
        public Session(Aspectus.Aspectus aspectus,
            DataMapper dataMapper,
            MappingManager mappingManager,
            SchemaManager schemaManager,
            QueryProviderManager queryProviderManager,
            Cache cacheManager,
            IEnumerable<IOptions<InflatableOptions>> options,
            IServiceProvider serviceProvider,
            ILogger<Session>? logger = null)
        {
            _MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
            _QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            Commands = [];
            Logger = logger;
            Options = options.FirstOrDefault()?.Value ?? InflatableOptions.Default;
            Cache = cacheManager?.GetOrAddCache(new InMemoryCacheOptions { MaxCacheSize = Options.MaxCacheSize, CompactionPercentage = .2, ScanFrequency = Options.ScanFrequency }, "Inflatable");
            Aspectus = aspectus;
            Services.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// The mapping manager
        /// </summary>
        private readonly MappingManager _MappingManager;

        /// <summary>
        /// The query provider manager
        /// </summary>
        private readonly QueryProviderManager _QueryProviderManager;

        /// <summary>
        /// Gets the aspectus.
        /// </summary>
        /// <value>The aspectus.</value>
        private Aspectus.Aspectus Aspectus { get; }

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        private ICache? Cache { get; }

        /// <summary>
        /// Gets or sets the commands.
        /// </summary>
        /// <value>The commands.</value>
        private IList<Commands.Interfaces.ICommand> Commands { get; }

        /// <summary>
        /// Gets the Logger.
        /// </summary>
        /// <value>The Logger.</value>
        private ILogger? Logger { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private InflatableOptions Options { get; }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void ClearCache() => Cache?.Compact(100);

        /// <summary>
        /// Adds the objects for deletion.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns>This.</returns>
        public ISession Delete<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            Commands.Add(new DeleteCommand(_MappingManager, _QueryProviderManager, Cache, objectsToDelete));
            return this;
        }

        /// <summary>
        /// Executes all queued commands.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public int Execute()
        {
            var Result = 0;
            RemoveDuplicateCommands();
            foreach (IMappingSource? Source in _MappingManager.WriteSources
                                                 .OrderBy(x => x.Order))
            {
                for (int X = 0, CommandsCount = Commands.Count; X < CommandsCount; ++X)
                {
                    Result += Commands[X].Execute(Source);
                }
            }
            Commands.Clear();
            return Result;
        }

        /// <summary>
        /// Executes all queued commands.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteAsync()
        {
            var Result = 0;
            RemoveDuplicateCommands();
            foreach (IMappingSource? Source in _MappingManager.WriteSources
                                                 .OrderBy(x => x.Order))
            {
                for (int X = 0, CommandsCount = Commands.Count; X < CommandsCount; ++X)
                {
                    Result += await Commands[X].ExecuteAsync(Source).ConfigureAwait(false);
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
            parameters ??= [];
            List<IParameter> Parameters = ConvertParameters(parameters);
            var KeyName = command + "_" + connection;
            Parameters.ForEach(x => KeyName = x.AddParameter(KeyName));
            if (QueryResults.TryGetCached(KeyName, Cache, out List<QueryResults>? CachedResults))
            {
                return CachedResults?.SelectMany(x => x.ConvertValues<TObject>()) ?? [];
            }
            IMappingSource? Source = Array.Find(_MappingManager.ReadSources, x => x.Source.Name == connection)
                ?? throw new ArgumentException($"Source not found {connection}");

            IEnumerable<IIDProperty> IDProperties = Source.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var ReturnValue = new List<Dynamo>();
            SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(Source.Source);
            _ = Batch.AddQuery(type, command, [.. Parameters]);
            Type ObjectType = Source.GetChildMappings(typeof(TObject)).First().ObjectType;
            try
            {
                List<QueryResults> Results = (await Batch.ExecuteAsync().ConfigureAwait(false)).ConvertAll(x => new QueryResults(new Query(ObjectType,
                                                                                                    CommandType.Text,
                                                                                                    command,
                                                                                                    QueryType.LinqQuery,
                                                                                                    [.. Parameters]),
                                                                                        x.Cast<Dynamo>(),
                                                                                        this,
                                                                                        Aspectus))
;

                QueryResults.CacheValues(KeyName, Results, Cache, Options);
                return [.. Results.SelectMany(x => x.ConvertValues<TObject>())];
            }
            catch
            {
                Logger?.LogDebug("Failed on query: {batch}", Batch);
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
            var KeyName = queries.Values.ToString(x => x + "_" + x.Source.Source.Name, "\n");
            _ = ((queries?.Values
                ?.SelectMany(x => x.Parameters)
                ?.Distinct()
                ?? [])
                ?.ForEach(x => KeyName = x.AddParameter(KeyName)));
            if (QueryResults.TryGetCached(KeyName, Cache, out List<QueryResults>? CachedResults))
            {
                return CachedResults?.SelectMany(x => x.ConvertValues<TObject>()) ?? [];
            }
            var Results = new List<QueryResults>();
            var FirstRun = true;
            IEnumerable<KeyValuePair<IMappingSource, QueryData<TObject>>> TempQueries = queries?.Where(x => x.Value.Source.CanRead && x.Value.Source.GetChildMappings(typeof(TObject)).Any()) ?? [];
            foreach (KeyValuePair<IMappingSource, QueryData<TObject>> Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (KeyValuePair<IMappingSource, QueryData<TObject>> Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator is null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            QueryResults.CacheValues(KeyName, Results, Cache, Options);
            return Results?.Select(x => x.ConvertValues<TObject>()).SelectMany(x => x)?.ToArray() ?? [];
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
            IEnumerable<KeyValuePair<IMappingSource, QueryData<TObject>>> TempQueries = queries.Where(x => x.Value.Source.CanRead && x.Value.Source.GetChildMappings(typeof(TObject)).Any());
            foreach (KeyValuePair<IMappingSource, QueryData<TObject>> Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (KeyValuePair<IMappingSource, QueryData<TObject>> Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator is null)
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
            parameters ??= [];
            List<IParameter> Parameters = ConvertParameters(parameters);
            IMappingSource? Source = Array.Find(_MappingManager.ReadSources, x => x.Source.Name == connection) ?? throw new ArgumentException($"Source not found {connection}");

            SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(Source.Source);
            _ = Batch.AddQuery(type, command, [.. Parameters]);
            try
            {
                return (await Batch.ExecuteAsync().ConfigureAwait(false))[0];
            }
            catch
            {
                Logger?.LogDebug("Failed on query: {batch}", Batch);
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
        /// <exception cref="ArgumentException">Source not found {connection}</exception>
        public Task<TObject> ExecuteScalarAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters ??= [];
            List<IParameter> Parameters = ConvertParameters(parameters);
            IMappingSource? Source = Array.Find(_MappingManager.ReadSources, x => x.Source.Name == connection) ?? throw new ArgumentException($"Source not found {connection}");

            var ReturnValue = new List<Dynamo>();
            SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(Source.Source);

            _ = Batch.AddQuery(type, command, [.. Parameters]);
            try
            {
                return Batch.ExecuteScalarAsync<TObject>()!;
            }
            catch
            {
                Logger?.LogDebug("Failed on query: {batch}", Batch);
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
            var Results = new List<QueryResults>();
            foreach (IMappingSource? Source in _MappingManager.ReadSources.Where(x => x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(Source.Source);
                QueryProvider.Interfaces.IGenerator<TObject>? Generator = _QueryProviderManager.CreateGenerator<TObject>(Source);
                IClassProperty? Property = FindProperty<TObject, TData>(Source, propertyName);
                QueryProvider.Interfaces.IQuery[] Queries = Generator?.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property) ?? [];
                for (int X = 0, QueriesLength = Queries.Length; X < QueriesLength; X++)
                {
                    QueryProvider.Interfaces.IQuery TempQuery = Queries[X];
                    _ = Batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
                }
                List<List<dynamic>>? ResultLists = null;

                try
                {
                    ResultLists = AsyncHelper.RunSync(Batch.ExecuteAsync);
                }
                catch
                {
                    Logger?.LogDebug("Failed on query: {batch}", Batch);
                    throw;
                }
                for (int X = 0, ResultListsCount = ResultLists.Count; X < ResultListsCount; ++X)
                {
                    if (X >= Queries.Length)
                        continue;
                    IEnumerable<IIDProperty> IDProperties = Source.GetParentMapping(Queries[X].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[X], ResultLists[X].Cast<Dynamo>(), this, Aspectus);
                    QueryResults? Result = Results.Find(y => y.CanCopy(TempQuery, IDProperties));
                    if (Result != null)
                    {
                        Result.CopyOrAdd(TempQuery, IDProperties);
                    }
                    else
                    {
                        Results.Add(TempQuery);
                    }
                }
            }
            return Results.SelectMany(x => x.ConvertValues<TData>()).ToObservableList(x => x);
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
            var Results = new List<QueryResults>();
            foreach (IMappingSource? Source in _MappingManager.ReadSources.Where(x => x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(Source.Source);
                QueryProvider.Interfaces.IGenerator<TObject>? Generator = _QueryProviderManager.CreateGenerator<TObject>(Source);
                IClassProperty? Property = FindProperty<TObject, TData>(Source, propertyName);
                QueryProvider.Interfaces.IQuery[] Queries = Generator?.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property) ?? [];
                for (int X = 0, QueriesLength = Queries.Length; X < QueriesLength; X++)
                {
                    QueryProvider.Interfaces.IQuery TempQuery = Queries[X];
                    _ = Batch.AddQuery(TempQuery.DatabaseCommandType, TempQuery.QueryString, TempQuery.Parameters!);
                }

                List<List<dynamic>>? ResultLists = null;

                try
                {
                    ResultLists = await Batch.ExecuteAsync().ConfigureAwait(false);
                }
                catch
                {
                    Logger?.LogDebug("Failed on query: {batch}", Batch);
                    throw;
                }

                for (int X = 0, ResultListsCount = ResultLists.Count; X < ResultListsCount; ++X)
                {
                    IEnumerable<IIDProperty> IDProperties = Source.GetParentMapping(Queries[X].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[X], ResultLists[X].Cast<Dynamo>(), this, Aspectus);
                    QueryResults? Result = Results.Find(y => y.CanCopy(TempQuery, IDProperties));
                    if (Result != null)
                    {
                        Result.CopyOrAdd(TempQuery, IDProperties);
                    }
                    else
                    {
                        Results.Add(TempQuery);
                    }
                }
            }
            return Results.SelectMany(x => x.ConvertValues<TData>()).ToObservableList(x => x);
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The appropriate property value</returns>
        public TData? LoadProperty<TObject, TData>(TObject objectToLoadProperty, string propertyName)
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
        public async Task<TData?> LoadPropertyAsync<TObject, TData>(TObject objectToLoadProperty, string propertyName)
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
            Commands.Add(new SaveCommand(_MappingManager, _QueryProviderManager, Cache, objectsToSave));
            return this;
        }

        /// <summary>
        /// Converts the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static List<IParameter> ConvertParameters(object[] parameters)
        {
            var Parameters = new List<IParameter>();
            for (int X = 0, ParametersLength = parameters.Length; X < ParametersLength; X++)
            {
                var CurrentParameter = parameters[X];
                if (CurrentParameter is IParameter TempQueryParameter)
                {
                    Parameters.Add(TempQueryParameter);
                }
                else if (CurrentParameter is null)
                {
                    Parameters.Add(new Parameter<object>(Parameters.Count.ToString(CultureInfo.InvariantCulture), null!));
                }
                else if (CurrentParameter is string TempParameter)
                {
                    Parameters.Add(new StringParameter(Parameters.Count.ToString(CultureInfo.InvariantCulture), TempParameter));
                }
                else
                {
                    Parameters.Add(new Parameter<object>(Parameters.Count.ToString(CultureInfo.InvariantCulture), CurrentParameter));
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
        private static IClassProperty? FindProperty<TObject, TData>(IMappingSource source, string propertyName)
            where TObject : class
            where TData : class
        {
            IEnumerable<Interfaces.IMapping> ParentMappings = source.GetChildMappings(typeof(TObject)).SelectMany(x => source.GetParentMapping(x.ObjectType)).Distinct();
            IClassProperty? Property = ParentMappings.SelectMany(x => x.ManyToManyProperties).FirstOrDefault(x => x.Name == propertyName);
            if (Property is not null)
            {
                return Property;
            }

            Property = ParentMappings.SelectMany(x => x.ManyToOneProperties).FirstOrDefault(x => x.Name == propertyName);
            return Property ?? ParentMappings.SelectMany(x => x.MapProperties).FirstOrDefault(x => x.Name == propertyName);
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
            QueryProvider.Interfaces.IGenerator<TObject>? Generator = _QueryProviderManager.CreateGenerator<TObject>(source.Key);
            QueryProvider.Interfaces.IQuery[] ResultingQueries = Generator?.GenerateQueries(source.Value) ?? [];
            SQLHelperDB.SQLHelper Batch = _QueryProviderManager.CreateBatch(source.Key.Source);
            for (int X = 0, ResultingQueriesLength = ResultingQueries.Length; X < ResultingQueriesLength; X++)
            {
                QueryProvider.Interfaces.IQuery ResultingQuery = ResultingQueries[X];
                _ = Batch.AddQuery(ResultingQuery.DatabaseCommandType, ResultingQuery.QueryString, ResultingQuery.Parameters!);
            }

            List<List<dynamic>>? Result = null;
            try
            {
                Result = await Batch.ExecuteAsync().ConfigureAwait(false);
            }
            catch
            {
                Logger?.LogDebug("Failed on query: {batch}", Batch);
                throw;
            }
            for (int X = 0, ResultCount = Result.Count; X < ResultCount; ++X)
            {
                IEnumerable<IIDProperty> IDProperties = source.Key.GetParentMapping(ResultingQueries[X].ReturnType).SelectMany(y => y.IDProperties);
                var TempResult = new QueryResults(ResultingQueries[X], Result[X].Cast<Dynamo>(), this, Aspectus);
                QueryResults? CopyResult = results.Find(y => y.CanCopy(TempResult, IDProperties));
                if (CopyResult is null && firstRun)
                {
                    results.Add(TempResult);
                }
                else if (firstRun)
                {
                    CopyResult?.CopyOrAdd(TempResult, IDProperties);
                }
                else
                {
                    CopyResult?.Copy(TempResult, IDProperties);
                }
            }
        }

        /// <summary>
        /// Removes the duplicate commands.
        /// </summary>
        private void RemoveDuplicateCommands()
        {
            var CommandsCount = Commands.Count;
            for (var X = 0; X < CommandsCount; ++X)
            {
                for (var Y = X + 1; Y < CommandsCount; ++Y)
                {
                    if (Commands[X].Merge(Commands[Y]))
                    {
                        Commands.RemoveAt(Y);
                        --Y;
                        --CommandsCount;
                    }
                }
            }
        }
    }
}