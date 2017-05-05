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
    public class ORMAspect : IAspect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMAspect"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies the aspect needs to use.</param>
        /// <param name="classManager">The class manager.</param>
        /// <param name="startMethodHelpers">The start method helpers.</param>
        /// <exception cref="System.ArgumentNullException">classManager</exception>
        /// <exception cref="ArgumentNullException">classManager</exception>
        public ORMAspect(ORMAspectAssemblies assemblies, MappingManager classManager, IEnumerable<IStartMethodHelper> startMethodHelpers)
        {
            StartMethodHelpers = startMethodHelpers ?? new List<IStartMethodHelper>();
            ClassManager = classManager ?? throw new ArgumentNullException(nameof(classManager));
            assemblies = assemblies ?? new ORMAspectAssemblies();
            AssembliesUsing.Add(assemblies.Assemblies);
            AssembliesUsing.Add(MetadataReference.CreateFromFile(typeof(ORMAspect).GetTypeInfo().Assembly.Location));
            AssembliesUsing.Add(MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).GetTypeInfo().Assembly.Location));
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
        /// List of interfaces that need to be injected by this aspect
        /// </summary>
        public ICollection<Type> InterfacesUsing => new Type[] { typeof(IORMObject) };

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
            foreach (var Source in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType)))
            {
                var Mapping = Source.Mappings[baseType];
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
            return "";
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
            Builder.AppendLine(@"public Session Session0{ get; set; }");
            Builder.AppendLine(@"public IList<string> PropertiesChanged0{ get; set; }");
            if (!type.Is<INotifyPropertyChanged>())
            {
                Builder.AppendLine(@"private PropertyChangedEventHandler propertyChanged_;
public event PropertyChangedEventHandler PropertyChanged
{
    add
    {
        propertyChanged_-=value;
        propertyChanged_+=value;
    }

    remove
    {
        propertyChanged_-=value;
    }
}");
                Builder.AppendLine(@"private void NotifyPropertyChanged0([CallerMemberName]string propertyName="""")
{
    var Handler = propertyChanged_;
    if (Handler != null)
        Handler(this, new PropertyChangedEventArgs(propertyName));
}");
            }
            Builder.AppendLine(SetupFields(type));
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
            foreach (var Source in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(baseType)))
            {
                var Mapping = Source.Mappings[baseType];
                var Builder = new StringBuilder();
                foreach (var Helper in StartMethodHelpers)
                {
                    Helper.Setup(method, Mapping, Builder);
                }
            }
            return "";
        }

        private static string SetupIEnumerableProperty(string returnValueName, IProperty property)
        {
            Contract.Requires<ArgumentNullException>(property != null, "Property");
            Contract.Requires<ArgumentNullException>(property.Mapping != null, "Property.Mapping");
            Contract.Requires<ArgumentNullException>(property.Mapping.ObjectType != null, "Property.Mapping.ObjectType");
            var Builder = new StringBuilder();
            Builder.AppendLineFormat("if(!{0}&&Session0!=null)", property.DerivedFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadProperties<{1},{2}>(this,\"{3}\");",
                        property.DerivedFieldName,
                        property.Mapping.ObjectType.GetName(),
                        property.Type.GetName(),
                        property.Name)
                .AppendLineFormat("{0}=true;", property.DerivedFieldName + "Loaded")
                .AppendLineFormat("((ObservableList<{1}>){0}).CollectionChanged += (x, y) => NotifyPropertyChanged0(\"{2}\");", property.DerivedFieldName, property.Type.GetName(), property.Name)
                .AppendLineFormat("((ObservableList<{1}>){0}).ForEach(TempObject => {{ ((IORMObject)TempObject).PropertyChanged += (x, y) => ((ObservableList<{1}>){0}).NotifyObjectChanged(x); }});", property.DerivedFieldName, property.Type.GetName())
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    property.DerivedFieldName);
            return Builder.ToString();
        }

        private static string SetupListProperty(string returnValueName, IProperty property)
        {
            Contract.Requires<ArgumentNullException>(property != null, "Property");
            Contract.Requires<ArgumentNullException>(property.Mapping != null, "Property.Mapping");
            Contract.Requires<ArgumentNullException>(property.Mapping.ObjectType != null, "Property.Mapping.ObjectType");
            var Builder = new StringBuilder();
            Builder.AppendLineFormat("if(!{0}&&Session0!=null)", property.DerivedFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadProperties<{1},{2}>(this,\"{3}\").ToList();",
                        property.DerivedFieldName,
                        property.Mapping.ObjectType.GetName(),
                        property.Type.GetName(),
                        property.Name)
                .AppendLineFormat("{0}=true;", property.DerivedFieldName + "Loaded")
                .AppendLineFormat("NotifyPropertyChanged0(\"{0}\");", property.Name)
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    property.DerivedFieldName);
            return Builder.ToString();
        }

        private static string SetupSingleProperty(string returnValueName, IProperty property)
        {
            Contract.Requires<ArgumentNullException>(property != null, "Property");
            Contract.Requires<ArgumentNullException>(property.Mapping != null, "Property.Mapping");
            Contract.Requires<ArgumentNullException>(property.Mapping.ObjectType != null, "Property.Mapping.ObjectType");
            var Builder = new StringBuilder();
            Builder.AppendLineFormat("if(!{0}&&Session0!=null)", property.DerivedFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadProperty<{1},{2}>(this,\"{3}\");",
                        property.DerivedFieldName,
                        property.Mapping.ObjectType.GetName(),
                        property.Type.GetName(),
                        property.Name)
                .AppendLineFormat("{0}=true;", property.DerivedFieldName + "Loaded")
                .AppendLineFormat("if({0}!=null)", property.DerivedFieldName)
                .AppendLine("{")
                .AppendLineFormat("({0} as INotifyPropertyChanged).PropertyChanged+=(x,y)=>NotifyPropertyChanged0(\"{1}\");", property.DerivedFieldName, property.Name)
                .AppendLine("}")
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    property.DerivedFieldName);
            return Builder.ToString();
        }

        private string SetupReferenceFields(Type type)
        {
            ReferenceFields = new List<IProperty>();
            var Builder = new StringBuilder();
            foreach (var Source in ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(type)))
            {
                var Mapping = Source.Mappings[type];
                foreach (IProperty Property in Mapping.ReferenceProperties.Where(x => !ReferenceFields.Contains(x)))
                {
                    ReferenceFields.Add(Property);
                    Builder.AppendLineFormat("private {0} {1};", Property.TypeName, Property.InternalFieldName);
                    Builder.AppendLineFormat("private bool {0};", Property.InternalFieldName + "Loaded");
                }
            }
            return Builder.ToString();
        }
    }
}