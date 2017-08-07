using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MapProperties
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public AllReferencesAndID MappedClass { get; set; }
    }
}