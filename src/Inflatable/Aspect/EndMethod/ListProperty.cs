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

using BigBook;
using Inflatable.Aspect.Interfaces;
using Inflatable.Interfaces;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect.EndMethod
{
    /// <summary>
    /// List property set up
    /// </summary>
    /// <seealso cref="IEndMethodHelper"/>
    public class ListProperty : IEndMethodHelper
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
            var Property = mapping.IDProperties.FirstOrDefault(x => x.Name == method.Name.Replace("get_", ""));
            if (Property == null)
                return;
            return;
            //if(!(Property is List))
            // return;
            var Builder = new StringBuilder();
            Builder.AppendLineFormat("if(!{0}&&Session0!=null)", Property.InternalFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadProperties<{1},{2}>(this,\"{3}\").ToList();",
                        Property.InternalFieldName,
                        Property.ParentMapping.ObjectType.GetName(),
                        Property.TypeName,
                        Property.Name)
                .AppendLineFormat("{0}=true;", Property.InternalFieldName + "Loaded")
                .AppendLineFormat("NotifyPropertyChanged0(\"{0}\");", Property.Name)
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    Property.InternalFieldName);
        }
    }
}