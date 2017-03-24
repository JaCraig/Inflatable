using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass2 : BaseClass1, IInterface2
    {
        [IntGenerator]
        public override int BaseClassValue1 { get; set; }

        [IntGenerator]
        public int InterfaceValue { get; set; }
    }
}