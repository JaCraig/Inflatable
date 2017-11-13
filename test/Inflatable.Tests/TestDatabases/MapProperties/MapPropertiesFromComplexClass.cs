using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public interface IMapPropertiesInterface
    {
        bool BoolValue { get; set; }
        int ID { get; set; }
    }

    public class MapPropertiesFromComplexClass : IMapPropertiesInterface
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual AllReferencesAndID MappedClass { get; set; }
    }
}