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

using BigBook.Queryable;
using BigBook.Queryable.BaseClasses;
using Inflatable.ClassMapper;
using Inflatable.LinqExpression;
using Inflatable.Sessions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Inflatable
{
    /// <summary>
    /// Db Context
    /// </summary>
    public class DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        public DbContext()
        {
            InternalSession = Canister.Builder.Bootstrapper?.Resolve<ISession>();
        }

        /// <summary>
        /// Gets or sets the internal session.
        /// </summary>
        /// <value>The internal session.</value>
        private ISession? InternalSession { get; }

        /// <summary>
        /// Executes the query asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects returned by the query</returns>
        public static Task<IEnumerable<dynamic>> ExecuteAsync(string command, CommandType type, string connection, params object[] parameters) => Canister.Builder.Bootstrapper?.Resolve<ISession>().ExecuteDynamicAsync(command, type, connection, parameters) ?? Task.FromResult((IEnumerable<dynamic>)Array.Empty<dynamic>());

        /// <summary>
        /// Adds a delete command.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToDelete">The objects to delete.</param>
        /// <returns>This</returns>
        public DbContext Delete<TObject>(params TObject[] objectsToDelete)
            where TObject : class
        {
            InternalSession?.Delete(objectsToDelete);
            return this;
        }

        /// <summary>
        /// Executes the various save and delete commands asynchronous.
        /// </summary>
        /// <returns>The number of rows modified or the first ID if inserting new items.</returns>
        public Task<int> ExecuteAsync() => InternalSession?.ExecuteAsync() ?? Task.FromResult(0);

        /// <summary>
        /// Adds a save command.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="objectsToSave">The objects to save.</param>
        /// <returns>This</returns>
        public DbContext Save<TObject>(params TObject[] objectsToSave)
            where TObject : class
        {
            InternalSession?.Save(objectsToSave);
            return this;
        }
    }

    /// <summary>
    /// Db context
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="QueryProviderBase"/>
    public class DbContext<TObject> : QueryProviderBase
        where TObject : class
    {
        /// <summary>
        /// Creates a query.
        /// </summary>
        /// <returns>The resulting query.</returns>
        public static IQueryable<TObject> CreateQuery() => new Query<TObject>(new DbContext<TObject>());

        /// <summary>
        /// Executes the query asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of objects returned by the query.</returns>
        public static Task<IEnumerable<TObject>> ExecuteAsync(string command, CommandType type, string connection, params object[] parameters) => Canister.Builder.Bootstrapper?.Resolve<ISession>().ExecuteAsync<TObject>(command, type, connection, parameters) ?? Task.FromResult((IEnumerable<TObject>)Array.Empty<TObject>());

        /// <summary>
        /// Executes the query getting a scalar asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="type">The type.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The first object returned by the query.</returns>
        public static Task<TObject> ExecuteScalarAsync(string command, CommandType type, string connection, params object[] parameters) => Canister.Builder.Bootstrapper?.Resolve<ISession>().ExecuteScalarAsync<TObject>(command, type, connection, parameters)!;

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public override object? Execute(Expression expression)
        {
            var Results = Translate(expression);
            var TempSession = Canister.Builder.Bootstrapper?.Resolve<ISession>();
            if (TempSession is null)
                return null;
            if (Results.Values.Any(x => x?.Count ?? false))
                return Task.Run(async () => await TempSession.ExecuteCountAsync(Results).ConfigureAwait(false)).GetAwaiter().GetResult();
            var DatabaseValues = Task.Run(async () => await TempSession.ExecuteAsync(Results).ConfigureAwait(false)).GetAwaiter().GetResult() ?? Array.Empty<dynamic>();
            return Results.Values.Any(x => (x?.Top ?? 0) == 1) ? DatabaseValues.FirstOrDefault() : DatabaseValues;
        }

        /// <summary>
        /// Gets the query text.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The query as a string</returns>
        public override string GetQueryText(Expression expression) => Translate(expression).First().Value.ToString();

        /// <summary>
        /// Translates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The translated expression</returns>
        private static IDictionary<IMappingSource, QueryData<TObject>> Translate(Expression expression) => Canister.Builder.Bootstrapper?.Resolve<QueryTranslator<TObject>>().Translate(expression) ?? new Dictionary<IMappingSource, QueryData<TObject>>();
    }
}