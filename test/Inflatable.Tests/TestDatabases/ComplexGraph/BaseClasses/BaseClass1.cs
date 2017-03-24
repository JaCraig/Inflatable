using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses
{
    public abstract class BaseClass1 : IInterface1
    {
        [IntGenerator]
        public abstract int BaseClassValue1 { get; set; }

        [IntGenerator]
        public int ID { get; set; }
    }
}