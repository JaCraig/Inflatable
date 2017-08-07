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
using BigBook;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect
{
    /// <summary>
    /// ORM Aspect
    /// </summary>
    /// <seealso cref="IAspect"/>
    public class ORMAspect : IAspect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMAspect"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies the aspect needs to use.</param>
        /// <param name="classManager">The class manager.</param>
        /// <param name="startMethodHelpers">The start method helpers.</param>
        /// <param name="interfaceImplementationHelpers">The interface implementation helpers.</param>
        /// <param name="endMethodHelpers">The end method helpers.</param>
        /// <exception cref="System.ArgumentNullException">classManager</exception>
        public ORMAspect(ORMAspectAssembliesBase assemblies,
            MappingManager classManager,
            IEnumerable<IStartMethodHelper> startMethodHelpers,
            IEnumerable<IInterfaceImplementationHelper> interfaceImplementationHelpers,
            IEnumerable<IEndMethodHelper> endMethodHelpers)
        {
            AssembliesUsing = new List<MetadataReference>();
            EndMethodHelpers = endMethodHelpers ?? new List<IEndMethodHelper>();
            InterfaceImplementationHelpers = interfaceImplementationHelpers ?? new List<IInterfaceImplementationHelper>();
            StartMethodHelpers = startMethodHelpers ?? new List<IStartMethodHelper>();
            ClassManager = classManager ?? throw new ArgumentNullException(nameof(classManager));
            assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            AssembliesUsing.Add(assemblies.Assemblies);
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(ORMAspect).GetTypeInfo().Assembly.Location));
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).GetTypeInfo().Assembly.Location));
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(Dynamo).GetTypeInfo().Assembly.Location));
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location));
            AssembliesUsing.AddIfUnique(MetadataReference.CreateFromFile(typeof(MulticastDelegate).GetTypeInfo().Assembly.Location));
            IDFields = new List<IIDProperty>();
            ReferenceFields = new List<IProperty>();
        }

        /// <summary>
        /// Set of assemblies that the aspect requires
        /// </summary>
        public ICollection<MetadataReference> AssembliesUsing { get; private set; }

        /// <summary>
        /// Gets the class manager.
        /// </summary>
        /// <value>The class manager.</value>
        public MappingManager ClassManager { get; }

        /// <summary>
        /// Gets the end method helpers.
        /// </summary>
        /// <value>The end method helpers.</value>
        public IEnumerable<IEndMethodHelper> EndMethodHelpers { get; }

        /// <summary>
        /// Gets or sets the identifier fields that have been completed already.
        /// </summary>
        /// <value>The identifier fields that have been completed already.</value>
        public List<IIDProperty> IDFields { get; set; }

        /// <summary>
        /// Gets the interface implementation helpers.
        /// </summary>
        /// <value>The interface implementation helpers.</value>
        public IEnumerable<IInterfaceImplementationHelper> InterfaceImplementationHelpers { get; }

        /// <summary>
        /// List of interfaces that need to be injected by this aspect
        /// </summary>
        public ICollection<Type> InterfacesUsing => new Type[] { typeof(IORMObject) };

        /// <summary>
        /// Gets or sets the map fields.
        /// </summary>
        /// <value>The map fields.</value>
        public List<IMapProperty> MapFields { get; set; }

        /// <summary>
        /// The reference fields that have been completed already.
        /// </summary>
        /// <value>The reference fields that have been completed already.</value>
        public List<IProperty> ReferenceFields { get; set; }

        /// <summary>
        /// Gets the start method helpers.
        /// </summary>
        /// <value>The start method helpers.</value>
        public IEnumerable<IStartMethodHelper> StartMethodHelpers { get; }

        /// <summary>
        /// Using statements that the aspect requires
        /// </summary>
        public ICollection<string> Usings => new string[]
        {
            "Inflatable",
            "Inflatable.Sessions",
            "BigBook",
            "System.Collections.Generic",
            "System.ComponentModel",
            "System.Runtime.CompilerServices"
        };

        /// <summary>
        /// Used to hook into the object once it has been created
        /// </summary>
        /// <param name="value">Object created by the system</param>
        public void Setup(object value)
        {
            var TempObject = (IORMObject)value;
            TempObject.PropertiesChanged0 = new List<string>();
            TempObject.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                var x = (IORMObject)sender;
                x.PropertiesChanged0.Add(e.PropertyName);
            };
        }

        /// <summary>
        /// Used to insert code into the default constructor
        /// </summary>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        public string SetupDefaultConstructor(Type baseType)
        {
            return "";
        }

        /// <summary>
        /// Used to insert code at the end of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <param name="returnValueName">Local holder for the value returned by the function</param>
        /// <returns>The code to insert</returns>
        public string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName)
        {
            if (!method.Name.StartsWith("get_", StringComparison.Ordinal))
                return "";
            var Builder = new StringBuilder();
            foreach (var Source in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType)))
            {
                foreach (var ParentType in Source.ParentTypes[baseType])
                {
                    var Mapping = Source.Mappings[ParentType];
                    foreach (var Helper in EndMethodHelpers)
                    {
                        Helper.Setup(returnValueName, method, Mapping, Builder);
                    }
                }
            }
            return Builder.ToString();
            //var Property = Mapping.Properties.FirstOrDefault(x => x.Name == Method.Name.Replace("get_", ""));
            //if (Property != null)
            //{
            //    var Builder = new StringBuilder();
            //    if (Property is IManyToOne || Property is IMap)
            //        Builder.AppendLine(SetupSingleProperty(ReturnValueName, Property));
            //    else if (Property is IIEnumerableManyToOne || Property is IManyToMany
            //        || Property is IIListManyToMany || Property is IIListManyToOne
            //        || Property is ICollectionManyToMany || Property is ICollectionManyToOne)
            //        Builder.AppendLine(SetupIEnumerableProperty(ReturnValueName, Property));
            //    else if (Property is IListManyToMany || Property is IListManyToOne)
            //        Builder.AppendLine(SetupListProperty(ReturnValueName, Property));
            //    return Builder.ToString();
            //}
        }

        /// <summary>
        /// Used to insert code within the catch portion of the try/catch portion of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        public string SetupExceptionMethod(MethodInfo method, Type baseType)
        {
            return "var Exception=CaughtException;";
        }

        /// <summary>
        /// Sets up the interfaces.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The code required to set them up.</returns>
        public string SetupInterfaces(Type type)
        {
            var Builder = new StringBuilder();
            foreach (var InterfaceHelper in InterfaceImplementationHelpers)
            {
                Builder.AppendLine(InterfaceHelper.Setup(type, this));
            }
            return Builder.ToString();
        }

        /// <summary>
        /// Used to insert code at the beginning of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        public string SetupStartMethod(MethodInfo method, Type baseType)
        {
            if (!method.Name.StartsWith("set_", StringComparison.Ordinal))
                return "";
            var Builder = new StringBuilder();
            foreach (var Source in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType)))
            {
                foreach (var ParentType in Source.ParentTypes[baseType])
                {
                    var Mapping = Source.Mappings[ParentType];
                    foreach (var Helper in StartMethodHelpers)
                    {
                        Helper.Setup(method, Mapping, Builder);
                    }
                }
            }
            return Builder.ToString();
        }
    }
}