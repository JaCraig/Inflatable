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
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="classManager">The class manager.</param>
        /// <param name="startMethodHelpers">The start method helpers.</param>
        /// <param name="interfaceImplementationHelpers">The interface implementation helpers.</param>
        /// <param name="endMethodHelpers">The end method helpers.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">classManager</exception>
        /// <exception cref="ArgumentNullException">classManager</exception>
        public ORMAspect(MappingManager classManager,
            IEnumerable<IStartMethodHelper> startMethodHelpers,
            IEnumerable<IInterfaceImplementationHelper> interfaceImplementationHelpers,
            IEnumerable<IEndMethodHelper> endMethodHelpers,
            ObjectPool<StringBuilder> objectPool)
        {
            AssembliesUsing = new List<MetadataReference>();
            IDFields = new List<IIDProperty>();
            ReferenceFields = new List<IProperty>();
            ManyToManyFields = new List<IManyToManyProperty>();
            ManyToOneFields = new List<IManyToOneProperty>();
            MapFields = new List<IMapProperty>();

            EndMethodHelpers = endMethodHelpers ?? Array.Empty<IEndMethodHelper>();
            InterfaceImplementationHelpers = interfaceImplementationHelpers ?? Array.Empty<IInterfaceImplementationHelper>();
            StartMethodHelpers = startMethodHelpers ?? Array.Empty<IStartMethodHelper>();
            ClassManager = classManager ?? throw new ArgumentNullException(nameof(classManager));
            ObjectPool = objectPool;

            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(ORMAspect).Assembly.Location));
            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location));
            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(Dynamo).Assembly.Location));
            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(MulticastDelegate).Assembly.Location));
            AssembliesUsing.AddIfUnique((x, y) => x.Display == y.Display, MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location));
        }

        /// <summary>
        /// The exception caught constant
        /// </summary>
        private const string ExceptionCaughtConst = "var Exception=CaughtException;";

        /// <summary>
        /// Set of assemblies that the aspect requires
        /// </summary>
        public ICollection<MetadataReference> AssembliesUsing { get; }

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
        public List<IIDProperty> IDFields { get; }

        /// <summary>
        /// Gets the interface implementation helpers.
        /// </summary>
        /// <value>The interface implementation helpers.</value>
        public IEnumerable<IInterfaceImplementationHelper> InterfaceImplementationHelpers { get; }

        /// <summary>
        /// List of interfaces that need to be injected by this aspect
        /// </summary>
        public ICollection<Type> InterfacesUsing { get; } = new Type[] { typeof(IORMObject) };

        /// <summary>
        /// Gets or sets the many to many fields.
        /// </summary>
        /// <value>The many to many fields.</value>
        public List<IManyToManyProperty> ManyToManyFields { get; }

        /// <summary>
        /// Gets or sets the many to one fields.
        /// </summary>
        /// <value>The many to one fields.</value>
        public List<IManyToOneProperty> ManyToOneFields { get; }

        /// <summary>
        /// Gets or sets the map fields.
        /// </summary>
        /// <value>The map fields.</value>
        public List<IMapProperty> MapFields { get; }

        /// <summary>
        /// Gets the object pool.
        /// </summary>
        /// <value>The object pool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// The reference fields that have been completed already.
        /// </summary>
        /// <value>The reference fields that have been completed already.</value>
        public List<IProperty> ReferenceFields { get; }

        /// <summary>
        /// Gets the start method helpers.
        /// </summary>
        /// <value>The start method helpers.</value>
        public IEnumerable<IStartMethodHelper> StartMethodHelpers { get; }

        /// <summary>
        /// Using statements that the aspect requires
        /// </summary>
        public ICollection<string> Usings { get; } = new string[]
        {
            "Inflatable",
            "Inflatable.Sessions",
            "Inflatable.Aspect.Interfaces",
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
            if (!(value is IORMObject TempObject))
                return;
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
        public string SetupDefaultConstructor(Type baseType) => string.Empty;

        /// <summary>
        /// Used to insert code at the end of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <param name="returnValueName">Local holder for the value returned by the function</param>
        /// <returns>The code to insert</returns>
        public string SetupEndMethod(MethodInfo method, Type baseType, string returnValueName)
        {
            if (method?.Name.StartsWith("get_", StringComparison.Ordinal) != true)
                return string.Empty;
            var Builder = ObjectPool.Get();
            foreach (var (Source, ParentType) in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType))
                .SelectMany(Source => Source.ParentTypes[baseType]
                .Select(ParentType => (Source, ParentType))))
            {
                var Mapping = Source.Mappings[ParentType];
                foreach (var Helper in EndMethodHelpers)
                {
                    Helper.Setup(returnValueName, method, Mapping, Builder);
                }
            }

            var Result = Builder.ToString();
            ObjectPool.Return(Builder);
            return Result;
        }

        /// <summary>
        /// Used to insert code within the catch portion of the try/catch portion of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        public string SetupExceptionMethod(MethodInfo method, Type baseType) => ExceptionCaughtConst;

        /// <summary>
        /// Sets up the interfaces.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The code required to set them up.</returns>
        public string SetupInterfaces(Type type)
        {
            var Builder = ObjectPool.Get();
            foreach (var InterfaceHelper in InterfaceImplementationHelpers)
            {
                Builder.AppendLine(InterfaceHelper.Setup(type, this, ObjectPool));
            }
            var Result = Builder.ToString();
            ObjectPool.Return(Builder);
            return Result;
        }

        /// <summary>
        /// Used to insert code at the beginning of the method
        /// </summary>
        /// <param name="method">Overridding Method</param>
        /// <param name="baseType">Base type</param>
        /// <returns>The code to insert</returns>
        public string SetupStartMethod(MethodInfo method, Type baseType)
        {
            if (method?.Name.StartsWith("set_", StringComparison.Ordinal) != true)
            {
                return string.Empty;
            }

            var Builder = ObjectPool.Get();
            foreach (var (Source, ParentType) in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType)).SelectMany(Source => Source.ParentTypes[baseType].Select(ParentType => (Source, ParentType))))
            {
                var Mapping = Source.Mappings[ParentType];
                foreach (var Helper in StartMethodHelpers)
                {
                    Helper.Setup(method, Mapping, Builder);
                }
            }

            var Result = Builder.ToString();
            ObjectPool.Return(Builder);
            return Result;
        }
    }
}