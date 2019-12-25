using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MapPropertiesWithBaseClasses
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual MapPropertyBaseClass MappedClass { get; set; }
    }

    public class MapProperty1 : MapPropertyBaseClass
    {
        public int ChildValue1 { get; set; }
    }

    public class MapProperty2 : MapPropertyBaseClass
    {
        public int ChildValue2 { get; set; }
    }

    public abstract class MapPropertyBaseClass : IMapPropertyInterface
    {
        public int ID { get; set; }

        public int BaseValue1 { get; set; }
    }

    public interface IMapPropertyInterface
    {
        int ID { get; set; }
    }
}
