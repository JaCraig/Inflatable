using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass3 : IInterface1
    {
        public int ID { get; set; }
        public int MyUniqueProperty { get; set; }
    }
}