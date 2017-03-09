using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass1 : BaseClass1
    {
        public override int BaseClassValue1 { get; set; }

        public int Value1 { get; set; }
    }
}