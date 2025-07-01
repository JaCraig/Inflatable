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

using Inflatable.ClassMapper;
using Inflatable.LinqExpression;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Inflatable.Sessions
{
    /// <summary>
    /// Session interface
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Clears the cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Deletes the specified objects to delete.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns></returns>
        ISession Delete<TObject>(params TObject[] objectsToDelete) where TObject : class;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        int Execute();

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<int> ExecuteAsync();

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries.</param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> ExecuteAsync<TObject>(IDictionary<IMappingSource, QueryData<TObject>> queries) where TObject : class;

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<TObject>> ExecuteAsync<TObject>(string command, CommandType type, string connection, params object[] parameters) where TObject : class;

        /// <summary>
        /// Executes the count asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="queries">The queries.</param>
        /// <returns></returns>
        Task<int> ExecuteCountAsync<TObject>(IDictionary<IMappingSource, QueryData<TObject>> queries) where TObject : class;

        /// <summary>
        /// Executes the dynamic asynchronous.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> ExecuteDynamicAsync(string command, CommandType type, string connection, params object[] parameters);

        /// <summary>
        /// Executes the scalar asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        Task<TObject> ExecuteScalarAsync<TObject>(string command, CommandType type, string connection, params object[] parameters);

        /// <summary>
        /// Loads the properties.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        IList<TData> LoadProperties<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class;

        /// <summary>
        /// Loads the properties asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        Task<IList<TData>> LoadPropertiesAsync<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class;

        /// <summary>
        /// Loads the property.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        TData? LoadProperty<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class;

        /// <summary>
        /// Loads the property asynchronous.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="objectToLoadProperty">The object to load property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        Task<TData?> LoadPropertyAsync<TObject, TData>(TObject objectToLoadProperty, string propertyName)
            where TObject : class
            where TData : class;

        /// <summary>
        /// Saves the specified objects to save.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToSave">The objects to save.</param>
        /// <returns></returns>
        ISession Save<TObject>(params TObject[] objectsToSave) where TObject : class;
    }
}