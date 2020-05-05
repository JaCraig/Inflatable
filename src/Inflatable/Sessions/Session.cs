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
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using Inflatable.Sessions.Commands;
using Serilog;
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
            DynamoFactory dynamoFactory)
        {
            MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            Commands = new List<Commands.Interfaces.ICommand>();
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Cache = cacheManager?.Cache();
            DynamoFactory = dynamoFactory;
        }

        /// <summary>
        /// Gets the dynamo factory.
        /// </summary>
        /// <value>The dynamo factory.</value>
        public DynamoFactory DynamoFactory { get; }

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        private ICache Cache { get; }

        /// <summary>
        /// Gets or sets the commands.
        /// </summary>
        /// <value>The commands.</value>
        private IList<Commands.Interfaces.ICommand> Commands { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// The mapping manager
        /// </summary>
        private readonly MappingManager MappingManager;

        /// <summary>
        /// The query provider manager
        /// </summary>
        private readonly QueryProviderManager QueryProviderManager;

        /// <summary>
        /// Adds the objects for deletion.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns>This.</returns>
        public ISession Delete<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            Commands.Add(new DeleteCommand(MappingManager, QueryProviderManager, Cache, objectsToDelete));
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
            foreach (var Source in MappingManager.Sources
                                                 .Where(x => x.CanWrite)
                                                 .OrderBy(x => x.Order))
            {
                for (int x = 0, CommandsCount = Commands.Count; x < CommandsCount; ++x)
                {
                    Result += Commands[x].Execute(Source, DynamoFactory);
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
            foreach (var Source in MappingManager.Sources
                                                 .Where(x => x.CanWrite)
                                                 .OrderBy(x => x.Order))
            {
                for (int x = 0, CommandsCount = Commands.Count; x < CommandsCount; ++x)
                {
                    Result += await Commands[x].ExecuteAsync(Source, DynamoFactory).ConfigureAwait(false);
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
            var Parameters = ConvertParameters(parameters);
            var KeyName = command + "_" + connection;
            Parameters.ForEach(x => KeyName = x.AddParameter(KeyName));
            if (QueryResults.IsCached(KeyName, Cache))
            {
                return QueryResults.GetCached(KeyName, Cache).SelectMany(x => x.ConvertValues<TObject>());
            }
            var Source = Array.Find(MappingManager.Sources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }

            var IDProperties = Source.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
            Batch.AddQuery(type, command, Parameters.ToArray());
            var ObjectType = Source.GetChildMappings(typeof(TObject)).First().ObjectType;
            try
            {
                var Results = (await Batch.ExecuteAsync().ConfigureAwait(false)).Select(x => new QueryResults(new Query(ObjectType,
                                                                                                    CommandType.Text,
                                                                                                    command,
                                                                                                    QueryType.LinqQuery,
                                                                                                    Parameters.ToArray()),
                                                                                        x.Cast<Dynamo>(),
                                                                                        this))
                                                          .ToList();

                QueryResults.CacheValues(KeyName, Results, Cache);
                return Results.SelectMany(x => x.ConvertValues<TObject>()).ToArray();
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
            var KeyName = queries.Values.ToString(x => x + "_" + x.Source.Source.Name, "\n");
            (queries?.Values
                ?.SelectMany(x => x.Parameters)
                ?.Distinct()
                ?? Array.Empty<IParameter>())
                ?.ForEach(x => KeyName = x.AddParameter(KeyName));
            if (QueryResults.IsCached(KeyName, Cache))
            {
                return QueryResults.GetCached(KeyName, Cache)?.SelectMany(x => x.ConvertValues<TObject>()) ?? Array.Empty<TObject>();
            }
            var Results = new List<QueryResults>();
            var FirstRun = true;
            var TempQueries = queries.Where(x => x.Value.Source.CanRead && x.Value.Source.GetChildMappings(typeof(TObject)).Any());
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator is null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            QueryResults.CacheValues(KeyName, Results, Cache);
            return Results?.SelectMany(x => x.ConvertValues<TObject>())?.ToArray() ?? Array.Empty<TObject>();
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
            var TempQueries = queries.Where(x => x.Value.Source.CanRead && x.Value.Source.GetChildMappings(typeof(TObject)).Any());
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source).ConfigureAwait(false);
                FirstRun = false;
            }
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator is null)
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
            var Source = Array.Find(MappingManager.Sources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }

            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
            Batch.AddQuery(type, command, Parameters.ToArray());
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
            var Source = Array.Find(MappingManager.Sources, x => x.Source.Name == connection);
            if (Source is null)
            {
                throw new ArgumentException($"Source not found {connection}");
            }

            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);

            Batch.AddQuery(type, command, Parameters.ToArray());
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
            var Results = new List<QueryResults>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
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
                    ResultLists = Task.Run(async () => await Batch.ExecuteAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                }
                catch
                {
                    Logger.Debug("Failed on query: " + Batch.ToString());
                    throw;
                }
                for (int x = 0, ResultListsCount = ResultLists.Count; x < ResultListsCount; ++x)
                {
                    if (x >= Queries.Length)
                        continue;
                    var IDProperties = Source.GetParentMapping(Queries[x].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[x], ResultLists[x].Cast<Dynamo>(), this);
                    var Result = Results.Find(y => y.CanCopy(TempQuery, IDProperties));
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
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source, DynamoFactory);
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
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

                for (int x = 0, ResultListsCount = ResultLists.Count; x < ResultListsCount; ++x)
                {
                    var IDProperties = Source.GetParentMapping(Queries[x].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[x], ResultLists[x].Cast<Dynamo>(), this);
                    var Result = Results.Find(y => y.CanCopy(TempQuery, IDProperties));
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
            Commands.Add(new SaveCommand(MappingManager, QueryProviderManager, Cache, objectsToSave));
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
            for (int x = 0, parametersLength = parameters.Length; x < parametersLength; x++)
            {
                var CurrentParameter = parameters[x];
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
        private static IClassProperty FindProperty<TObject, TData>(IMappingSource source, string propertyName)
            where TObject : class
            where TData : class
        {
            var ParentMappings = source.GetChildMappings(typeof(TObject)).SelectMany(x => source.GetParentMapping(x.ObjectType)).Distinct();
            IClassProperty Property = ParentMappings.SelectMany(x => x.ManyToManyProperties).FirstOrDefault(x => x.Name == propertyName);
            if (Property != null)
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
            var Generator = QueryProviderManager.CreateGenerator<TObject>(source.Key);
            var ResultingQueries = Generator.GenerateQueries(source.Value);
            var Batch = QueryProviderManager.CreateBatch(source.Key.Source, DynamoFactory);
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
            for (int x = 0, ResultCount = Result.Count; x < ResultCount; ++x)
            {
                var IDProperties = source.Key.GetParentMapping(ResultingQueries[x].ReturnType).SelectMany(y => y.IDProperties);
                var TempResult = new QueryResults(ResultingQueries[x], Result[x].Cast<Dynamo>(), this);
                var CopyResult = results.Find(y => y.CanCopy(TempResult, IDProperties));
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