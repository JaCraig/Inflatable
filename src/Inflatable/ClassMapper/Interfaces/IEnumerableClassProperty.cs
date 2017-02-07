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

using BigBook.DataMapper.Interfaces;
using System.Data;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// IEnumerable class property interface
    /// </summary>
    public interface IEnumerableClassProperty
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IProperty"/> is set to cascade on delete/save.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        bool Cascade { get; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        IMapping ForeignMapping { get; }

        /// <summary>
        /// Gets the load command.
        /// </summary>
        /// <value>The load command.</value>
        string LoadCommand { get; }

        /// <summary>
        /// Gets the type of the load command.
        /// </summary>
        /// <value>The type of the load command.</value>
        CommandType LoadCommandType { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        string TableName { get; }
    }
}