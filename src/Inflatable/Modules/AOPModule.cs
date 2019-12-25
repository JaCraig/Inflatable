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

using Aspectus.Interfaces;
using Inflatable.ClassMapper;
using System;
using System.Linq;

namespace Inflatable.Modules
{
    /// <summary>
    /// AOP Module
    /// </summary>
    /// <seealso cref="IAOPModule"/>
    public class AOPModule : IAOPModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AOPModule"/> class.
        /// </summary>
        /// <param name="classManager">The class manager.</param>
        /// <exception cref="ArgumentNullException">classManager</exception>
        public AOPModule(MappingManager classManager)
        {
            ClassManager = classManager ?? throw new ArgumentNullException(nameof(classManager));
        }

        /// <summary>
        /// Gets the class manager.
        /// </summary>
        /// <value>The class manager.</value>
        public MappingManager ClassManager { get; }

        /// <summary>
        /// Used to add a class to the AOP system
        /// </summary>
        /// <param name="manager">AOP manager</param>
        public void Setup(Aspectus.Aspectus manager)
        {
            manager.Setup(ClassManager.Sources.SelectMany(x => x.ConcreteTypes).Distinct().ToArray());
            manager.FinalizeSetup();
        }
    }
}