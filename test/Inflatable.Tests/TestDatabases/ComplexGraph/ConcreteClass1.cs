using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass1 : BaseClass1
    {
        [IntGenerator]
        public override int BaseClassValue1 { get; set; }

        [IntGenerator]
        public int Value1 { get; set; }
    }
}