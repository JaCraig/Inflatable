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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inflatable.Aspect.EndMethod
{
    public class IManyToOnePropertyLazyLoad : IEndMethodHelper
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
            var Property = mapping.ManyToOneProperties.FirstOrDefault(x => x.Name == method.Name.Replace("get_", ""));
            if (Property == null)
                return;
            if (Property is IManyToOneListProperty)
                LoadList(returnValueName, method, mapping, builder, Property);
            else
                LoadSingle(returnValueName, method, mapping, builder, Property);
        }

        private void LoadList(string returnValueName, MethodInfo method, IMapping mapping, StringBuilder builder, IManyToOneProperty property)
        {
            builder.AppendLineFormat("if(!{0}&&Session0!=null)", property.InternalFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadPropertiesAsync<{1},{2}>(this,\"{3}\").GetAwaiter().GetResult();",
                        property.InternalFieldName,
                        property.ParentMapping.ObjectType.GetName(),
                        property.TypeName,
                        property.Name)
                .AppendLineFormat("{0}=true;", property.InternalFieldName + "Loaded")
                .AppendLineFormat("if({0}!=null)", property.InternalFieldName)
                .AppendLine("{")
                .AppendLineFormat("((ObservableList<{1}>){0}).CollectionChanged += (x, y) => NotifyPropertyChanged0(\"{2}\");", property.InternalFieldName, property.TypeName, property.Name)
                .AppendLineFormat(@"((ObservableList<{1}>){0}).ForEach(TempObject => {{
    ((IORMObject)TempObject).PropertyChanged += (x, y) => ((ObservableList<{1}>){0}).NotifyObjectChanged(x);
}});", property.InternalFieldName, property.TypeName)
                .AppendLine("}")
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    property.InternalFieldName);
        }

        private void LoadSingle(string returnValueName, MethodInfo method, IMapping mapping, StringBuilder builder, IManyToOneProperty property)
        {
            builder.AppendLineFormat("if(!{0}&&Session0!=null)", property.InternalFieldName + "Loaded")
                .AppendLine("{")
                .AppendLineFormat("{0}=Session0.LoadPropertyAsync<{1},{2}>(this,\"{3}\").GetAwaiter().GetResult();",
                        property.InternalFieldName,
                        property.ParentMapping.ObjectType.GetName(),
                        property.TypeName,
                        property.Name)
                .AppendLineFormat("{0}=true;", property.InternalFieldName + "Loaded")
                .AppendLineFormat("if({0}!=null)", property.InternalFieldName)
                .AppendLine("{")
                .AppendLineFormat("({0} as INotifyPropertyChanged).PropertyChanged+=(x,y)=>NotifyPropertyChanged0(\"{1}\");", property.InternalFieldName, property.Name)
                .AppendLine("}")
                .AppendLine("}")
                .AppendLineFormat("{0}={1};",
                    returnValueName,
                    property.InternalFieldName);
        }
    }
}