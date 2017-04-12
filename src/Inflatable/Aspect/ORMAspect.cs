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
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inflatable.Aspect
{
    /// <summary>
    /// ORM Aspect
    /// </summary>
    public class ORMAspect : IAspect
    {
        /// <summary>
        /// Set of assemblies that the aspect requires
        /// </summary>
        public ICollection<MetadataReference> AssembliesUsing { get; private set; }

        /// <summary>
        /// List of interfaces that need to be injected by this aspect
        /// </summary>
        public ICollection<Type> InterfacesUsing => new Type[] { typeof(IORMObject) };

        /// <summary>
        /// Using statements that the aspect requires
        /// </summary>
        public ICollection<string> Usings => new string[]
        {
            "Inflatable",
            "Inflatable.Sessions",
            "BigBook",
            "System.Collections.Generic",
            "System.ComponentModel"
        };

        /// <summary>
        /// Gets or sets the identifier fields that have been completed already.
        /// </summary>
        /// <value>The identifier fields that have been completed already.</value>
        private List<IIDProperty> IDFields { get; set; }

        /// <summary>
        /// The reference fields that have been completed already.
        /// </summary>
        /// <value>The reference fields that have been completed already.</value>
        private List<IProperty> ReferenceFields { get; set; }

        public void Setup(object value)
        {
            throw new NotImplementedException();
        }

        public string SetupDefaultConstructor(Type baseType)
        {
            throw new NotImplementedException();
        }

        public string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName)
        {
            throw new NotImplementedException();
        }

        public string SetupExceptionMethod(MethodInfo method, Type baseType)
        {
            throw new NotImplementedException();
        }

        public string SetupInterfaces(Type type)
        {
            throw new NotImplementedException();
        }

        public string SetupStartMethod(MethodInfo method, Type baseType)
        {
            throw new NotImplementedException();
        }
    }
}