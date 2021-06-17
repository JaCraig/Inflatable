using Inflatable.Aspect.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;

namespace Inflatable.Aspect.InterfaceImplementation
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Inflatable.Aspect.Interfaces.IInterfaceImplementationHelper"/>
    public class SetupIORMInitializationMethod : IInterfaceImplementationHelper
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; } = int.MaxValue;

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
            Builder.AppendLine(@"public void InitializeORMObject0(ISession session)
{
    Session0 = session;");

            foreach (var Field in aspect.ManyToManyFields)
            {
                Builder.AppendLine(Field.InternalFieldName + "Loaded = false;");
            }
            foreach (var Field in aspect.ManyToOneFields)
            {
                Builder.AppendLine(Field.InternalFieldName + "Loaded = false;");
            }
            foreach (var Field in aspect.MapFields)
            {
                Builder.AppendLine(Field.InternalFieldName + "Loaded = false;");
            }
            foreach (var Field in aspect.ReferenceFields)
            {
                Builder.AppendLine(Field.InternalFieldName + "Loaded = false;");
            }
            Builder.AppendLine("PropertiesChanged0.Clear();");
            Builder.AppendLine("}");
            var Result = Builder.ToString();
            objectPool.Return(Builder);
            return Result;
        }
    }
}