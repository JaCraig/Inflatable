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

using System;
using System.ComponentModel.DataAnnotations;
using Valkyrie;

namespace Inflatable.Interfaces
{
    /// <summary>
    /// Object interface
    /// </summary>
    public interface IObject<IDType>
    {
        /// <summary>
        /// Is this item active?
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Date created
        /// </summary>
        [Required]
        [Between("1/1/1900", "1/1/2100", "Date created is not valid")]
        DateTime DateCreated { get; set; }

        /// <summary>
        /// Date last modified
        /// </summary>
        [Required]
        [Between("1/1/1900", "1/1/2100", "Date modified is not valid")]
        DateTime DateModified { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        IDType ID { get; set; }
    }
}