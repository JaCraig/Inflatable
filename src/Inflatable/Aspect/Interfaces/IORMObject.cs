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

using Inflatable.Sessions;
using System.Collections.Generic;
using System.ComponentModel;

namespace Inflatable.Aspect.Interfaces
{
    /// <summary>
    /// ORM object interface
    /// </summary>
    public interface IORMObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the properties changed.
        /// </summary>
        /// <value>The properties changed.</value>
        IList<string> PropertiesChanged0 { get; set; }

        /// <summary>
        /// ORM session that this item came from (used for lazy loading)
        /// </summary>
        /// <value>The ORM session object.</value>
        ISession Session0 { get; set; }

        /// <summary>
        /// Initializes the orm object.
        /// </summary>
        /// <param name="session">The session.</param>
        void InitializeORMObject0(ISession session);
    }
}