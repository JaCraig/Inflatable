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

using Inflatable.ClassMapper.Column.Interfaces;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Interface holding column information
    /// </summary>
    public interface IPropertyColumns
    {
        /// <summary>
        /// Gets the column information.
        /// </summary>
        /// <returns>The column information.</returns>
        IQueryColumnInfo[] GetColumnInfo();

        /// <summary>
        /// Sets the column information.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        void SetColumnInfo(IMappingSource mappings);
    }
}