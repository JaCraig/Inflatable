using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
{
    public interface IManyToManyPropertyInterface
    {
        int ID { get; set; }
    }

    public class ManyToManyPropertiesWithBaseClasses
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual IList<ManyToManyPropertyBaseClass> ManyToManyClass { get; set; }
    }

    public class ManyToManyProperty1 : ManyToManyPropertyBaseClass
    {
        public int ChildValue1 { get; set; }
    }

    public class ManyToManyProperty2 : ManyToManyPropertyBaseClass
    {
        public int ChildValue2 { get; set; }
    }

    public abstract class ManyToManyPropertyBaseClass : IManyToManyPropertyInterface
    {
        public int BaseValue1 { get; set; }
        public int ID { get; set; }
    }
}