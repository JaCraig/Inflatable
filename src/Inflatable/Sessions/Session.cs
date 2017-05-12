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
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using SQLHelper.HelperClasses.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
        /// <exception cref="System.ArgumentNullException">
        /// mappingManager or schemaManager or queryProviderManager
        /// </exception>
        public Session(MappingManager mappingManager, SchemaManager schemaManager, QueryProviderManager queryProviderManager, Aspectus.Aspectus aopManager)
        {
            AOPManager = aopManager ?? throw new System.ArgumentNullException(nameof(aopManager));
            MappingManager = mappingManager ?? throw new System.ArgumentNullException(nameof(mappingManager));
            SchemaManager = schemaManager ?? throw new System.ArgumentNullException(nameof(schemaManager));
            QueryProviderManager = queryProviderManager ?? throw new System.ArgumentNullException(nameof(queryProviderManager));
            AOPManager.Setup(MappingManager.Sources.SelectMany(x => x.ConcreteTypes).ToArray());
        }

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
        /// Gets the aop manager.
        /// </summary>
        /// <value>The aop manager.</value>
        public Aspectus.Aspectus AOPManager { get; }

        /// <summary>
        /// Returns all items that match the criteria
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>All items that match the criteria</returns>
        public IEnumerable<TObject> All<TObject>(params IParameter[] parameters)
            where TObject : class
        {
            parameters = parameters ?? new IParameter[0];
            var ReturnValue = new List<Dynamo>();
            foreach (var Source in MappingManager.Sources.Where(x => x.CanRead).OrderBy(x => x.Order))
            {
                foreach (var Mapping in Source.GetChildMappings<TObject>())
                {
                    var ParentMappings = Source.GetParentMapping(typeof(TObject));
                    var IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
                    var Query = Mapping.Queries[QueryType.All];
                    foreach (Dynamo Item in QueryProviderManager.CreateBatch(Source.Source)
                                                                .AddQuery(Query.QueryString,
                                                                            Query.DatabaseCommandType,
                                                                            parameters)
                                                                .Execute()[0])
                    {
                        CopyOrAdd(ReturnValue, IDProperties, Item);
                    }
                }
            }
            return ConvertValues<TObject>(ReturnValue);
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
    }
}