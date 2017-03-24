using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass3 : IInterface1
    {
        [IntGenerator]
        public int ID { get; set; }

        [IntGenerator]
        public int MyUniqueProperty { get; set; }
    }
}