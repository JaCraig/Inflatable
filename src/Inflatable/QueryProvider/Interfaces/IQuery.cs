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
using System.Data;

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Holds an individual query's information.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets the type of the database command.
        /// </summary>
        /// <value>The type of the database command.</value>
        CommandType DatabaseCommandType { get; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <value>The query string.</value>
        string QueryString { get; }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        QueryType QueryType { get; }
    }
}