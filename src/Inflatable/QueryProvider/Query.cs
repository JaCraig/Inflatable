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

using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Data;

namespace Inflatable.QueryProvider
{
    /// <summary>
    /// Query holder
    /// </summary>
    /// <seealso cref="IQuery"/>
    public class Query : IQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Query"/> class.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="databaseCommandType">Type of the database command.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="parameters">The parameters.</param>
        public Query(Type returnType, CommandType databaseCommandType, string queryString, QueryType queryType, params IParameter[] parameters)
        {
            DatabaseCommandType = databaseCommandType;
            QueryType = queryType;
            QueryString = queryString;
            Parameters = parameters ?? new IParameter[0];
            ReturnType = returnType;
        }

        /// <summary>
        /// Gets the type of the database command.
        /// </summary>
        /// <value>The type of the database command.</value>
        public CommandType DatabaseCommandType { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public IParameter[] Parameters { get; set; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <value>The query string.</value>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public QueryType QueryType { get; set; }

        /// <summary>
        /// Gets the type of the return value.
        /// </summary>
        /// <value>The type of the return value.</value>
        public Type ReturnType { get; set; }
    }
}