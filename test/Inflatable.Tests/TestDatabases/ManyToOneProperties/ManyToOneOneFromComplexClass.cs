using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties
{
    public interface IManyToOneOne
    {
        bool BoolValue { get; set; }
        int ID { get; set; }
    }

    public class ManyToOneOneFromComplexClass : IManyToOneOne
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual AllReferencesAndID ManyToOneClass { get; set; }
    }
}