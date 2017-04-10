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
using Data.Modeler.Providers.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Auto ID
    /// </summary>
    /// <seealso cref="Inflatable.ClassMapper.Interfaces.IAutoIDProperty"/>
    public class AutoID : IAutoIDProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoID"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="parentMapping">The parent mapping.</param>
        public AutoID(string columnName, IMapping parentMapping)
        {
            ColumnName = columnName;
            ParentMapping = parentMapping;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        public IMapping ParentMapping { get; }

        /// <summary>
        /// Adds to child table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToChildTable(ITable table)
        {
            table.AddColumn<long>(ParentMapping.TableName + ColumnName,
                System.Data.DbType.Int64,
                0,
                false,
                false,
                false,
                false,
                true,
                ParentMapping.TableName,
                ColumnName,
                0,
                "",
                true,
                true);
        }

        /// <summary>
        /// Adds this instance to the table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToTable(ITable table)
        {
            table.AddColumn<long>(ColumnName, System.Data.DbType.Int64, identity: true, index: true, primaryKey: true);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public void Setup()
        {
        }

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString()
        {
            return typeof(long).GetName() + " " + ParentMapping + "." + ColumnName;
        }
    }
}