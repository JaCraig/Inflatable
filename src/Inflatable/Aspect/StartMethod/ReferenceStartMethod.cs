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
    /// Reference start method
    /// </summary>
    /// <seealso cref="IStartMethodHelper"/>
    public class ReferenceStartMethod : IStartMethodHelper
    {
        /// <summary>
        /// Sets up the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="Mapping">The mapping.</param>
        /// <param name="Builder">The builder.</param>
        public void Setup(MethodInfo method, IMapping Mapping, StringBuilder Builder)
        {
            var Property = Mapping.ReferenceProperties.FirstOrDefault(x => x.Name == method.Name.Replace("set_", ""));
            if (Property == null)
                return;
            Builder.AppendLineFormat("{0}=value;", Property.InternalFieldName);
            Builder.AppendLineFormat("NotifyPropertyChanged0(\"{0}\");", Property.Name);
        }
    }
}