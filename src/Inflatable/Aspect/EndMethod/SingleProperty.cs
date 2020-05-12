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
using Inflatable.Interfaces;
using System;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect.EndMethod
{
    /// <summary>
    /// Single mapped property
    /// </summary>
    /// <seealso cref="IEndMethodHelper"/>
    public class SingleProperty : IEndMethodHelper
    {
        /// <summary>
        /// Setups the specified return value name.
        /// </summary>
        /// <param name="returnValueName">Name of the return value.</param>
        /// <param name="method">The method.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="builder">The builder.</param>
        public void Setup(string returnValueName, MethodInfo method, IMapping mapping, StringBuilder builder)
        {
            if (mapping is null)
                return;
            var Property = mapping.MapProperties.Find(x => x.Name == method.Name.Replace("get_", string.Empty, StringComparison.Ordinal));
            if (Property is null)
            {
                return;
            }

            builder.AppendLineFormat("if(!{0}&&!(Session0 is null))", Property.InternalFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadProperty<{1},{2}>(this,\"{3}\");",
                        Property.InternalFieldName,
                        Property.ParentMapping.ObjectType.GetName(),
                        Property.TypeName,
                        Property.Name)
                .AppendLineFormat("{0}=true;", Property.InternalFieldName + "Loaded")
                .AppendLineFormat("if(!({0} is null))", Property.InternalFieldName)
                .AppendLine("{")
                .AppendLineFormat("({0} as INotifyPropertyChanged).PropertyChanged+=(x,y)=>NotifyPropertyChanged0(\"{1}\");", Property.InternalFieldName, Property.Name)
                .AppendLine("}")
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    Property.InternalFieldName);
        }
    }
}