﻿/*
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

namespace Inflatable.QueryProvider.Enums
{
    /// <summary>
    /// Query type enum
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// The insert
        /// </summary>
        Insert,

        /// <summary>
        /// The update
        /// </summary>
        Update,

        /// <summary>
        /// The delete
        /// </summary>
        Delete,

        /// <summary>
        /// The linq query
        /// </summary>
        LinqQuery,

        /// <summary>
        /// The load property
        /// </summary>
        LoadProperty,

        /// <summary>
        /// The joins delete
        /// </summary>
        JoinsDelete,

        /// <summary>
        /// The joins save
        /// </summary>
        JoinsSave,

        /// <summary>
        /// The load data
        /// </summary>
        LoadData
    }
}