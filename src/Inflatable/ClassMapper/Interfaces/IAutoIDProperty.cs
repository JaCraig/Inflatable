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

using Data.Modeler.Providers.Interfaces;
using Inflatable.Interfaces;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Auto ID property interface
    /// </summary>
    public interface IAutoIDProperty
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        IMapping ParentMapping { get; }

        /// <summary>
        /// Adds to child table.
        /// </summary>
        /// <param name="table">The table.</param>
        void AddToChildTable(ITable table);

        /// <summary>
        /// Adds this instance to the table.
        /// </summary>
        /// <param name="table">The table.</param>
        void AddToTable(ITable table);

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        void Setup();
    }
}