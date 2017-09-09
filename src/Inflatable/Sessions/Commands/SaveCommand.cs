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

using Inflatable.Sessions.Commands.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.Sessions.Commands.Enums;
using System.Threading.Tasks;
using BigBook;
using Inflatable.QueryProvider.Enums;
using System.Reflection;
using System.Linq;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Aspect.Interfaces;
using System.Collections;

namespace Inflatable.Sessions.Commands
{
    /// <summary>
    /// Save command
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="BaseClasses.CommandBaseClass{TObject}" />
    public class SaveCommand<TObject> : CommandBaseClass<TObject>
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommand{TObject}"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="objects">The objects.</param>
        public SaveCommand(MappingManager mappingManager, QueryProviderManager queryProviderManager, TObject[] objects) 
            : base(mappingManager, queryProviderManager, objects)
        {
        }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <value>
        /// The type of the command.
        /// </value>
        public override CommandType CommandType => CommandType.Save;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>
        /// The number of rows that are modified.
        /// </returns>
        public override async Task<int> Execute()
        {
            //Steps:
            //1) Cascade Map items (Insert items with identity based IDs in layer batch, others can go in the overall batch)
            //2) Save Many to Many items (Insert items with identity based IDs in layer batch, others can go in the overall batch)
            //3) Run the individual layer batch to get item IDs.
            //4) Save individual item (Insert items with identity based IDs in layer batch, others can go in the overall batch)
            //5) Run the individual layer batch to get the individual items' IDs.
            //6) Save/Delete joins as needed in overall batch.
            //7) Run the overall batch to update everything, save joins, and delete joins.

            return 0;
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
    }
}
