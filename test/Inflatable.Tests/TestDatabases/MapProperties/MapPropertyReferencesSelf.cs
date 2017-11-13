using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MapPropertyReferencesSelf
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual MapPropertyReferencesSelf MappedClass { get; set; }
    }
}