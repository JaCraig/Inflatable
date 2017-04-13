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

using Inflatable.DataSource;
using System.Data.Common;

namespace Inflatable.Interfaces
{
    /// <summary>
    /// Database configuration interface
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Name associated with the database/connection string
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Order that this database should be in (if only one database is being used, it is ignored)
        /// </summary>
        /// <value>The order.</value>
        int Order { get; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <value>The provider.</value>
        DbProviderFactory Provider { get; }

        /// <summary>
        /// Gets the source options.
        /// </summary>
        /// <value>The source options.</value>
        Options SourceOptions { get; }
    }
}