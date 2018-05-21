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
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect.StartMethod
{
    /// <summary>
    /// Map start method
    /// </summary>
    /// <seealso cref="Inflatable.Aspect.Interfaces.IStartMethodHelper"/>
    public class MapStartMethod : IStartMethodHelper
    {
        /// <summary>
        /// Sets up the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="builder">The builder.</param>
        public void Setup(MethodInfo method, IMapping mapping, StringBuilder builder)
        {
            var Property = mapping.MapProperties.FirstOrDefault(x => x.Name == method.Name.Replace("set_", ""));
            if (Property == null)
            {
                return;
            }

            builder.AppendLineFormat("{0} = value;", Property.InternalFieldName);
            builder.AppendLineFormat("{0} = true;", Property.InternalFieldName + "Loaded");
            builder.AppendLineFormat("NotifyPropertyChanged0(\"{0}\");", Property.Name);
        }
    }
}