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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Schema;
using Inflatable.Sessions.Commands;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections;
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
            return await new DeleteCommand<TObject>(MappingManager, QueryProviderManager, objectsToDelete).Execute();
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
                    await SaveCascade(objectsToInsert[x], TempType, Source, new List<object>(), Batch);
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
                IClassProperty Property = FindProperty<TObject, TData>(Source, propertyName);
                var Queries = Generator.GenerateQueries(QueryType.LoadProperty, objectToLoadProperty, Property);
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
                    await SaveCascade(objectsToUpdate[x], TempType, Source, new List<object>(), Batch);
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

        /// <summary>
        /// Converts the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
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
        /// Updates the cascade object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="MapValue">The map value.</param>
        /// <param name="TempType">Type of the temporary.</param>
        /// <param name="Generator">The generator.</param>
        /// <param name="Batch">The batch.</param>
        /// <returns></returns>
        private static void InsertCascadeObject(MappingSource source, object MapValue, Type TempType, IGenerator Generator, SQLHelper.SQLHelper Batch)
        {
            var TempQueries = Generator.GenerateDeclarations(QueryType.Insert);
            for (int x = 0; x < TempQueries.Length; ++x)
            {
                Batch.AddQuery(TempQueries[x].QueryString, TempQueries[x].DatabaseCommandType, TempQueries[x].Parameters);
            }
            var Queries = Generator.GenerateQueries(QueryType.Insert, MapValue);
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

        /// <summary>
        /// Inserts the cascade object.
        /// </summary>
        /// <param name="MapValue">The map value.</param>
        /// <param name="TempType">Type of the temporary.</param>
        /// <param name="Generator">The generator.</param>
        /// <param name="Batch">The batch.</param>
        /// <returns></returns>
        private static void UpdateCascadeObject(object MapValue, ref Type TempType, IGenerator Generator, SQLHelper.SQLHelper Batch)
        {
            TempType = TempType.GetTypeInfo().BaseType;
            var Queries = Generator.GenerateQueries(QueryType.Update, MapValue);
            foreach (var TempQuery in Queries)
            {
                Batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
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
            var ParentMappings = source.GetParentMapping(typeof(TObject));
            IClassProperty Property = ParentMappings.SelectMany(x => x.ManyToManyProperties).FirstOrDefault(x => x.Name == propertyName);
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
        private async Task SaveCascade(object @object, Type objectType, MappingSource source, IList<object> objectsSeen, SQLHelper.SQLHelper batch)
        {
            if (@object == null || objectsSeen.Contains(@object))
                return;
            objectsSeen.Add(@object);
            var ORMObject = @object as IORMObject;
            var ParentMappings = source.GetParentMapping(objectType);

            //Add cascade saving for ManyToMany as well as the Aspect code to lazy load them as needed. Also clean up various code below.
            //asdfasdf
            // Code below should only do inserts as a one time batch for the ID on Map items.
            // ManyToMany can be batched together into one set of inserts instead of a bunch. Updates
            // can just be added to the overall batch as that doesn't touch the IDs.
            foreach (var MapProperty in ParentMappings.SelectMany(x => x.MapProperties)
                                                      .Where(x => x.Cascade
                                                               && (ORMObject == null
                                                                  || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var MapValue = MapProperty.GetValue(@object);
                await SaveCascadeObject(source, objectsSeen, MapValue, batch);
            }
            foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties)
                                                             .Where(x => x.Cascade
                                                                      && (ORMObject == null
                                                                         || ORMObject.PropertiesChanged0.Contains(x.Name))))
            {
                var ManyToManyValue = ManyToManyProperty.GetValue(@object) as IEnumerable;
                foreach (var Item in ManyToManyValue)
                {
                    await SaveCascadeObject(source, objectsSeen, Item, batch);
                }
            }
            foreach (var ManyToManyProperty in ParentMappings.SelectMany(x => x.ManyToManyProperties))
            {
                SaveJoins(@object, source, batch, ManyToManyProperty);
            }
        }

        /// <summary>
        /// Saves the cascade object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="objectsSeen">The objects seen.</param>
        /// <param name="MapValue">The map value.</param>
        /// <returns></returns>
        private async Task SaveCascadeObject(MappingSource source, IList<object> objectsSeen, object MapValue, SQLHelper.SQLHelper batch)
        {
            if (MapValue == null || objectsSeen.Contains(MapValue))
                return;
            objectsSeen.Add(MapValue);
            Type TempType = MapValue.GetType();
            var CascadeType = TempType;
            if (CascadeType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                CascadeType = CascadeType.GetTypeInfo().BaseType;
            await SaveCascade(MapValue, CascadeType, source, objectsSeen, batch);

            var Generator = QueryProviderManager.CreateGenerator(MapValue.GetType(), source);
            if (MapValue is IORMObject UpdateMapObject)
            {
                UpdateCascadeObject(MapValue, ref TempType, Generator, batch);
            }
            else
            {
                var Batch = QueryProviderManager.CreateBatch(source.Source);
                InsertCascadeObject(source, MapValue, TempType, Generator, Batch);
                await Batch.ExecuteAsync();
            }
            QueryResults.RemoveCacheTag(TempType.GetName());
        }

        /// <summary>
        /// Saves the joins.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="ManyToManyProperty">The many to many property.</param>
        private void SaveJoins(object @object, MappingSource source, SQLHelper.SQLHelper batch, IManyToManyProperty ManyToManyProperty)
        {
            var LinksGenerator = QueryProviderManager.CreateGenerator(ManyToManyProperty.ParentMapping.ObjectType, source);
            var TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsDelete, @object, ManyToManyProperty);
            foreach (var TempQuery in TempQueries)
            {
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
            TempQueries = LinksGenerator.GenerateQueries(QueryType.JoinsSave, @object, ManyToManyProperty);
            foreach (var TempQuery in TempQueries)
            {
                batch.AddQuery(TempQuery.QueryString, TempQuery.DatabaseCommandType, TempQuery.Parameters);
            }
        }
    }
}