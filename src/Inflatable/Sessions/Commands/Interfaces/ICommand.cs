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
using System.Threading.Tasks;

namespace Inflatable.Sessions.Commands.Interfaces
{
    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        Enums.CommandType CommandType { get; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The number of rows that are modified.</returns>
        Task<int> Execute(MappingSource source);

        /// <summary>
        /// Merges the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>True if the items are merged, false otherwise.</returns>
        bool Merge(ICommand command);
    }
}