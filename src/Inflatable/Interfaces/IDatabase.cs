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

namespace Inflatable.Interfaces
{
    /// <summary>
    /// Database configuration interface
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Determines if audit tables are generated
        /// </summary>
        /// <value><c>true</c> if audit; otherwise, <c>false</c>.</value>
        bool Audit { get; }

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
        /// Should this database be used to read data?
        /// </summary>
        /// <value><c>true</c> if readable; otherwise, <c>false</c>.</value>
        bool Readable { get; }

        /// <summary>
        /// Should the structure of the database be updated?
        /// </summary>
        /// <value><c>true</c> if update; otherwise, <c>false</c>.</value>
        bool Update { get; }

        /// <summary>
        /// Should this database be used to write data?
        /// </summary>
        /// <value><c>true</c> if writable; otherwise, <c>false</c>.</value>
        bool Writable { get; }

        /// <summary>
        /// Gets a value indicating whether this source should be optimized automatically.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be optimized; otherwise, <c>false</c>.
        /// </value>
        bool Optimize { get; }
    }
}