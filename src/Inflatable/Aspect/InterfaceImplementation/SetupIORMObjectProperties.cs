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

using BigBook;
using Inflatable.Aspect.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.ComponentModel;
using System.Text;

namespace Inflatable.Aspect.InterfaceImplementation
{
    /// <summary>
    /// Sets up the IORM Object properties
    /// </summary>
    /// <seealso cref="IInterfaceImplementationHelper"/>
    public class SetupIORMObjectProperties : IInterfaceImplementationHelper
    {
        /// <summary>
        /// Setups the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <returns>The resulting code in string format.</returns>
        public string Setup(Type type, ORMAspect aspect, ObjectPool<StringBuilder> objectPool)
        {
            if (objectPool is null)
                return string.Empty;
            var Builder = objectPool.Get();
            Builder.AppendLine("public ISession Session0{ get; set; }")
                .AppendLine("public IList<string> PropertiesChanged0{ get; set; }");
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
}")
                    .AppendLine(@"private void NotifyPropertyChanged0([CallerMemberName]string propertyName="""")
{
    var Handler = propertyChanged_;
    if (Handler != null)
        Handler(this, new PropertyChangedEventArgs(propertyName));
}");
            }
            var Result = Builder.ToString();
            objectPool.Return(Builder);
            return Result;
        }
    }
}