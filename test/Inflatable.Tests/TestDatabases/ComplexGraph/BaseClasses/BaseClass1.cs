using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses
{
    public abstract class BaseClass1 : IInterface1
    {
        public abstract int BaseClassValue1 { get; set; }
        public int ID { get; set; }
    }
}