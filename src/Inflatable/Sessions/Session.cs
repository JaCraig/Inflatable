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
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Schema;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
            var ReturnValue = 0;
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                for (int x = 0; x < objectsToDelete.Length; ++x)
                {
                    var TempType = objectsToDelete[x].GetType();
                    if (TempType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                        TempType = TempType.GetTypeInfo().BaseType;
                    QueryResults.RemoveCacheTag(TempType.GetName());
                    DeleteCascade(objectsToDelete[x], Source, Batch);
                    var Generator = QueryProviderManager.CreateGenerator(objectsToDelete[x].GetType(), Source);
                    var Queries = Generator.GenerateQueries(QueryType.Delete, objectsToDelete[x]);
                    foreach (var TempQuery in Queries)
                    {
                        Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
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
            Batch.AddQuery(command, type, Parameters.ToArray());
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
            if (QueryResults.IsCached(KeyName))
            {
                return QueryResults.GetCached(KeyName).SelectMany(x => x.ConvertValues<TObject>());
            }
            List<QueryResults> Results = new List<QueryResults>();
            bool FirstRun = true;
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator != null
                                                   && x.Value.Source.CanRead
                                                   && x.Value.Source.Mappings.ContainsKey(typeof(TObject)))
                                          .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            foreach (var Source in queries.Where(x => x.Value.WhereClause.InternalOperator == null
                                                   && x.Value.Source.CanRead
                                                   && x.Value.Source.Mappings.ContainsKey(typeof(TObject)))
                                          .OrderBy(x => x.Key.Order))
            {
                await GenerateQueryAsync(Results, FirstRun, Source);
                FirstRun = false;
            }
            QueryResults.CacheValues(KeyName, Results);
            return Results.SelectMany(x => x.ConvertValues<TObject>()).ToArray();
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
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                var Generator = QueryProviderManager.CreateGenerator(typeof(TObject), Source);
                var DeclarationBatch = QueryProviderManager.CreateBatch(Source.Source);

                var DeclarationQuery = Generator.GenerateDeclarations(QueryType.Insert);
                for (int x = 0; x < DeclarationQuery.Length; ++x)
                {
                    DeclarationBatch.AddQuery(DeclarationQuery[x].QueryString, DeclarationQuery[x].DatabaseCommandType, DeclarationQuery[x].Parameters);
                }

                List<IMapping> ParentMappings = new List<IMapping>();
                foreach (var ChildMapping in Source.GetChildMappings(typeof(TObject)))
                {
                    ParentMappings.AddIfUnique(Source.GetParentMapping(ChildMapping.ObjectType));
                }
                var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);

                for (int x = 0; x < objectsToInsert.Length; ++x)
                {
                    var TempType = objectsToInsert[x].GetType();
                    if (TempType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                        TempType = TempType.GetTypeInfo().BaseType;
                    QueryResults.RemoveCacheTag(TempType.GetName());
                    await SaveCascade(objectsToInsert[x], TempType, Source, new List<object>());
                    Generator = QueryProviderManager.CreateGenerator(TempType, Source);
                    var ObjectQueries = Generator.GenerateQueries(QueryType.Insert, objectsToInsert[x]);
                    foreach (var ObjectQuery in ObjectQueries)
                    {
                        var IDProperty = IDProperties.FirstOrDefault(y => y.AutoIncrement);
                        var ReturnedID = Batch.AddQuery((Command, ResultList, InsertObject) =>
                                                        {
                                                            if (IDProperty != null && IDProperty.AutoIncrement)
                                                            {
                                                                IDProperty.SetValue(InsertObject, IDProperty.GetValue((Dynamo)ResultList[0]));
                                                            }
                                                        },
                                                        objectsToInsert[x],
                                                        ObjectQuery.QueryString,
                                                        ObjectQuery.DatabaseCommandType,
                                                        ObjectQuery.Parameters);
                    }
                }
                var FinalBatch = QueryProviderManager.CreateBatch(Source.Source);
                FinalBatch.AddQuery(DeclarationBatch.RemoveDuplicateCommands());
                FinalBatch.AddQuery(Batch);
                await FinalBatch.ExecuteAsync();
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
            List<QueryResults> Results = new List<QueryResults>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                var Generator = QueryProviderManager.CreateGenerator<TObject>(Source);
                var Queries = Generator.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, propertyName);
                foreach (var TempQuery in Queries)
                {
                    Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                }
                var ResultLists = await Batch.ExecuteAsync();
                for (int x = 0; x < ResultLists.Count; ++x)
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
            int ReturnValue = 0;
            foreach (var Source in MappingManager.Sources.Where(x => x.CanWrite
                                                                  && x.Mappings.ContainsKey(typeof(TObject)))
                                                         .OrderBy(x => x.Order))
            {
                var Batch = QueryProviderManager.CreateBatch(Source.Source);
                var DeclarationBatch = QueryProviderManager.CreateBatch(Source.Source);
                for (int x = 0; x < objectsToUpdate.Length; ++x)
                {
                    var TempType = objectsToUpdate[x].GetType();
                    if (TempType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                        TempType = TempType.GetTypeInfo().BaseType;
                    await SaveCascade(objectsToUpdate[x], TempType, Source, new List<object>());
                    var Generator = QueryProviderManager.CreateGenerator(TempType, Source);
                    QueryResults.RemoveCacheTag(TempType.GetName());
                    var Queries = Generator.GenerateQueries(QueryType.Update, objectsToUpdate[x]);
                    foreach (var TempQuery in Queries)
                    {
                        Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                    }
                }
                var FinalBatch = QueryProviderManager.CreateBatch(Source.Source);
                FinalBatch.AddQuery(DeclarationBatch.RemoveDuplicateCommands());
                FinalBatch.AddQuery(Batch);
                ReturnValue += await FinalBatch.ExecuteScalarAsync<int>();
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

        /// <summary>
        /// Deletes the cascade.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        private void DeleteCascade(object @object, MappingSource source, SQLHelper.SQLHelper batch)
        {
            foreach (var MapProperty in source.GetParentMapping(@object.GetType()).SelectMany(x => x.MapProperties).Where(x => x.Cascade))
            {
                var Generator = QueryProviderManager.CreateGenerator(MapProperty.PropertyType, source);
                var MapValue = MapProperty.GetValue(@object);
                if (MapValue != null)
                {
                    var TempType = MapValue.GetType();
                    if (TempType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                        TempType = TempType.GetTypeInfo().BaseType;
                    QueryResults.RemoveCacheTag(TempType.GetName());
                    DeleteCascade(MapValue, source, batch);
                    var Queries = Generator.GenerateQueries(QueryType.Delete, MapValue);
                    foreach (var TempQuery in Queries)
                    {
                        batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                    }
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
        private async Task GenerateQueryAsync<TObject>(List<QueryResults> results, bool firstRun, KeyValuePair<MappingSource, QueryData<TObject>> source)
            where TObject : class
        {
            var Generator = QueryProviderManager.CreateGenerator<TObject>(source.Key);
            var ResultingQueries = Generator.GenerateQueries(source.Value);
            var Batch = QueryProviderManager.CreateBatch(source.Key.Source);
            foreach (var ResultingQuery in ResultingQueries)
            {
                Batch.AddQuery(ResultingQuery.QueryString, ResultingQuery.DatabaseCommandType, ResultingQuery.Parameters);
            }
            var Result = await Batch.ExecuteAsync();
            for (int x = 0; x < Result.Count; ++x)
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

        /// <summary>
        /// Update cascade.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="source">The source.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <returns></returns>
        private async Task SaveCascade(object @object, Type objectType, MappingSource source, IList<object> objectsSeen)
        {
            if (@object == null || objectsSeen.Contains(@object))
                return;
            objectsSeen.Add(@object);
            var ORMObject = @object as IORMObject;
            foreach (var MapProperty in source.GetParentMapping(objectType)
                                              .SelectMany(x => x.MapProperties)
                                              .Where(x => x.Cascade
                                                       && (ORMObject == null
                                                          || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var MapValue = MapProperty.GetValue(@object);
                if (MapValue != null)
                {
                    Type TempType = MapValue.GetType();
                    var CascadeType = TempType;
                    if (CascadeType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                        CascadeType = CascadeType.GetTypeInfo().BaseType;
                    await SaveCascade(MapValue, CascadeType, source, objectsSeen);

                    var Generator = QueryProviderManager.CreateGenerator(MapValue.GetType(), source);
                    var Batch = QueryProviderManager.CreateBatch(source.Source);

                    IQuery[] Queries = null;
                    IORMObject UpdateMapObject = MapValue as IORMObject;
                    if (UpdateMapObject == null)
                    {
                        var TempQueries = Generator.GenerateDeclarations(QueryType.Insert);
                        for (int x = 0; x < TempQueries.Length; ++x)
                        {
                            Batch.AddQuery(TempQueries[x].QueryString, TempQueries[x].DatabaseCommandType, TempQueries[x].Parameters);
                        }
                        Queries = Generator.GenerateQueries(QueryType.Insert, MapValue);
                        foreach (var TempQuery in Queries)
                        {
                            var IDProperty = source.GetParentMapping(TempType)
                                                   .SelectMany(x => x.IDProperties)
                                                   .FirstOrDefault(x => x.AutoIncrement);
                            var ReturnedID = Batch.AddQuery((TmpCommand, ResultList, InsertObject) =>
                            {
                                if (IDProperty != null && IDProperty.AutoIncrement)
                                {
                                    IDProperty.SetValue(InsertObject, IDProperty.GetValue((Dynamo)ResultList[0]));
                                }
                            },
                            MapValue,
                            TempQuery.QueryString,
                            TempQuery.DatabaseCommandType,
                            TempQuery.Parameters);
                        }
                    }
                    else
                    {
                        TempType = TempType.GetTypeInfo().BaseType;
                        Queries = Generator.GenerateQueries(QueryType.Update, MapValue);
                        foreach (var TempQuery in Queries)
                        {
                            Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
                        }
                    }
                    QueryResults.RemoveCacheTag(TempType.GetName());
                    await Batch.ExecuteAsync();
                }
            }
        }
    }
}