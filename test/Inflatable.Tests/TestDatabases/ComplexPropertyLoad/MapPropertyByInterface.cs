using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexPropertyLoad
{
    public interface IMappedClass
    {
        int ID { get; set; }
    }

    public class MappedClass1 : IMappedClass
    {
        public int ID { get; set; }
    }

    public class MappedClass2 : IMappedClass
    {
        public int ID { get; set; }
    }

    public class MapPropertyByInterface
    {
        [IntGenerator]
        public int ID { get; set; }

        public virtual IMappedClass MappedClass { get; set; }
    }
}