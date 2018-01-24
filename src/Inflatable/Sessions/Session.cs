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
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using Inflatable.Sessions.Commands;
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
        /// <exception cref="ArgumentNullException">
        /// cacheManager or aopManager or mappingManager or schemaManager or queryProviderManager
        /// </exception>
        public Session(MappingManager mappingManager,
            SchemaManager schemaManager,
            QueryProviderManager queryProviderManager,
            Aspectus.Aspectus aopManager)
        {
            AOPManager = aopManager ?? throw new ArgumentNullException(nameof(aopManager));
            MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
            SchemaManager = schemaManager ?? throw new ArgumentNullException(nameof(schemaManager));
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            Commands = new List<Commands.Interfaces.ICommand>();
        }

        /// <summary>
        /// The query provider manager
        /// </summary>
        private readonly QueryProviderManager QueryProviderManager;

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
        /// The schema manager
        /// </summary>
        private SchemaManager SchemaManager;

        /// <summary>
        /// Gets or sets the commands.
        /// </summary>
        /// <value>The commands.</value>
        private IList<Commands.Interfaces.ICommand> Commands { get; set; }

        /// <summary>
        /// Adds the objects for deletion.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns>This.</returns>
        public Session Delete<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            Commands.Add(new DeleteCommand(MappingManager, QueryProviderManager, objectsToDelete));
            return this;
        }

        /// <summary>
        /// Executes all queued commands.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> ExecuteAsync()
        {
            int Result = 0;
            int CommandsCount = Commands.Count;
            for (int x = 0; x < CommandsCount; ++x)
            {
                for (int y = x + 1; y < CommandsCount; ++y)
                {
                    if (Commands[x].Merge(Commands[y]))
                    {
                        Commands.RemoveAt(y);
                        --y;
                        --CommandsCount;
                    }
                }
            }
            CommandsCount = Commands.Count;
            foreach (var Source in MappingManager.Sources
                                                 .Where(x => x.CanWrite)
                                                 .OrderBy(x => x.Order))
            {
                for (int x = 0; x < CommandsCount; ++x)
                {
                    Result += await Commands[x].Execute(Source);
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
        /// <exception cref="System.ArgumentException"></exception>
        public async Task<IEnumerable<TObject>> ExecuteAsync<TObject>(string command, CommandType type, string connection, params object[] parameters)
            where TObject : class
        {
            parameters = parameters ?? new IParameter[0];
            List<IParameter> Parameters = ConvertParameters(parameters);
            string KeyName = command + "_" + connection;
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (QueryResults.IsCached(KeyName))
            {
                return QueryResults.GetCached(KeyName).SelectMany(x => x.ConvertValues<TObject>());
            }
            var Source = MappingManager.Sources.FirstOrDefault(x => x.Source.Name == connection);
            if (Source == null)
                throw new ArgumentException($"Source not found {connection}");
            var IDProperties = Source.GetParentMapping(typeof(TObject)).SelectMany(x => x.IDProperties);
            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source);
            Batch.AddQuery(command, type, Parameters.ToArray());
            var ObjectType = Source.GetChildMappings(typeof(TObject)).First().ObjectType;
            var Results = (await Batch.ExecuteAsync()).Select(x => new QueryResults(new Query(ObjectType,
                                                                                                CommandType.Text,
                                                                                                command,
                                                                                                QueryType.LinqQuery,
                                                                                                Parameters.ToArray()),
                                                                                    x.Select(y => (Dynamo)y),
                                                                                    this))
                                                      .ToList();

            QueryResults.CacheValues(KeyName, Results);
            return Results.SelectMany(x => x.ConvertValues<TObject>()).ToArray();
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
            if (QueryResults.IsCached(KeyName))
            {
                return QueryResults.GetCached(KeyName).SelectMany(x => x.ConvertValues<TObject>());
            }
            List<QueryResults> Results = new List<QueryResults>();
            bool FirstRun = true;
            var TempQueries = queries.Where(x => x.Value.Source.CanRead && x.Value.Source.GetChildMappings(typeof(TObject)).Any());
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator != null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            foreach (var Source in TempQueries.Where(x => x.Value.WhereClause.InternalOperator == null)
                                              .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            QueryResults.CacheValues(KeyName, Results);
            return Results.SelectMany(x => x.ConvertValues<TObject>()).ToArray();
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
        public async Task<IEnumerable<dynamic>> ExecuteDynamicAsync(string command, CommandType type, string connection, params object[] parameters)
        {
            parameters = parameters ?? new IParameter[0];
            List<IParameter> Parameters = ConvertParameters(parameters);
            var Source = MappingManager.Sources.FirstOrDefault(x => x.Source.Name == connection);
            if (Source == null)
                throw new ArgumentException($"Source not found {connection}");
            var Batch = QueryProviderManager.CreateBatch(Source.Source);
            Batch.AddQuery(command, type, Parameters.ToArray());
            return (await Batch.ExecuteAsync())[0];
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
            var ReturnValue = new List<Dynamo>();
            var Batch = QueryProviderManager.CreateBatch(Source.Source);

            Batch.AddQuery(command, type, Parameters.ToArray());
            return await Batch.ExecuteScalarAsync<TObject>();
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
            List<QueryResults> Results = new List<QueryResults>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
                IClassProperty Property = FindProperty<TObject, TData>(Source, propertyName);
                var Queries = Generator.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property);
                for (int x = 0, QueriesLength = Queries.Length; x < QueriesLength; x++)
                {
                    var TempQuery = Queries[x];
                    Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                }

                var ResultLists = Batch.Execute();
                for (int x = 0, ResultListsCount = ResultLists.Count; x < ResultListsCount; ++x)
                {
                    var IDProperties = Source.GetParentMapping(Queries[x].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[x], ResultLists[x].Select(y => (Dynamo)y), this);
                    var Result = Results.FirstOrDefault(y => y.CanCopy(TempQuery, IDProperties));
                    if (Result != null)
                        Result.CopyOrAdd(TempQuery, IDProperties);
                    else
                        Results.Add(TempQuery);
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
            List<QueryResults> Results = new List<QueryResults>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
                IClassProperty Property = FindProperty<TObject, TData>(Source, propertyName);
                var Queries = Generator.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property);
                for (int x = 0, QueriesLength = Queries.Length; x < QueriesLength; x++)
                {
                    var TempQuery = Queries[x];
                    Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                }

                var ResultLists = await Batch.ExecuteAsync();
                for (int x = 0, ResultListsCount = ResultLists.Count; x < ResultListsCount; ++x)
                {
                    var IDProperties = Source.GetParentMapping(Queries[x].ReturnType).SelectMany(y => y.IDProperties);
                    var TempQuery = new QueryResults(Queries[x], ResultLists[x].Select(y => (Dynamo)y), this);
                    var Result = Results.FirstOrDefault(y => y.CanCopy(TempQuery, IDProperties));
                    if (Result != null)
                        Result.CopyOrAdd(TempQuery, IDProperties);
                    else
                        Results.Add(TempQuery);
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
            where TData : class
        {
            return LoadProperties<TObject, TData>(objectToLoadProperty, propertyName).FirstOrDefault();
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
        /// Adds the specified objects to save.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToSave">The objects to save.</param>
        /// <returns>This</returns>
        public Session Save<TObject>(params TObject[] objectsToSave)
            where TObject : class
        {
            Commands.Add(new SaveCommand(MappingManager, QueryProviderManager, objectsToSave));
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
                object CurrentParameter = parameters[x];
                var TempParameter = CurrentParameter as string;
                if (CurrentParameter is IParameter TempQueryParameter)
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

        /// <summary>
        /// Finds the property.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property</returns>
        private IClassProperty FindProperty<TObject, TData>(MappingSource source, string propertyName)
            where TObject : class
            where TData : class
        {
            var ParentMappings = source.GetChildMappings(typeof(TObject)).SelectMany(x => source.GetParentMapping(x.ObjectType)).Distinct();
            IClassProperty Property = ParentMappings.SelectMany(x => x.ManyToManyProperties).FirstOrDefault(x => x.Name == propertyName);
            if (Property != null)
                return Property;
            Property = ParentMappings.SelectMany(x => x.ManyToOneProperties).FirstOrDefault(x => x.Name == propertyName);
            if (Property != null)
                return Property;
            return ParentMappings.SelectMany(x => x.MapProperties).FirstOrDefault(x => x.Name == propertyName);
        }

        /// <summary>
        /// Generates the query asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="results">The results.</param>
        /// <param name="firstRun">if set to <c>true</c> [first run].</param>
        /// <param name="source">The source.</param>
        /// <returns>The results of the query on the source</returns>
        private async Task GenerateQueryAsync<TObject>(List<QueryResults> results, bool firstRun, KeyValuePair<MappingSource, QueryData<TObject>> source)
            where TObject : class
        {
            var Generator = QueryProviderManager.CreateGenerator<TObject>(source.Key);
            var ResultingQueries = Generator.GenerateQueries(source.Value);
            var Batch = QueryProviderManager.CreateBatch(source.Key.Source);
            for (int x = 0, ResultingQueriesLength = ResultingQueries.Length; x < ResultingQueriesLength; x++)
            {
                var ResultingQuery = ResultingQueries[x];
                Batch.AddQuery(ResultingQuery.QueryString, ResultingQuery.DatabaseCommandType, ResultingQuery.Parameters);
            }

            var Result = await Batch.ExecuteAsync();
            for (int x = 0, ResultCount = Result.Count; x < ResultCount; ++x)
            {
                var IDProperties = source.Key.GetParentMapping(ResultingQueries[x].ReturnType).SelectMany(y => y.IDProperties);
                var TempResult = new QueryResults(ResultingQueries[x], Result[x].Select(y => (Dynamo)y), this);
                var CopyResult = results.FirstOrDefault(y => y.CanCopy(TempResult, IDProperties));
                if (CopyResult == null && firstRun)
                    results.Add(TempResult);
                else if (firstRun)
                    CopyResult.CopyOrAdd(TempResult, IDProperties);
                else
                    CopyResult.Copy(TempResult, IDProperties);
            }
        }
    }
}