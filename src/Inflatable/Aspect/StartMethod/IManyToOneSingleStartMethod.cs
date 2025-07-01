using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inflatable.Aspect.StartMethod
{
    /// <summary>
    /// Many to one single property start method
    /// </summary>
    /// <seealso cref="IStartMethodHelper"/>
    public class IManyToOneSingleStartMethod : IStartMethodHelper
    {
        /// <summary>
        /// Sets up the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="builder">The builder.</param>
        public void Setup(MethodInfo method, IMapping mapping, StringBuilder builder)
        {
            if (mapping is null || builder is null)
                return;
            var Property = mapping.ManyToOneProperties.Find(x => x.Name == method.Name.Replace("set_", "", StringComparison.Ordinal));
            if (Property is null)
                return;

            if (Property is IManyToOneListProperty)
                return;

            builder.Append(Property.InternalFieldName).AppendLine(" = value;")
                .Append(Property.InternalFieldName).AppendLine("Loaded = true;")
                .Append("NotifyPropertyChanged0(\"").Append(Property.Name).AppendLine("\");");
        }
    }
}