using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;

namespace Inflatable.Tests.TestDatabases.ComplexGraph
{
    public class ConcreteClass2 : BaseClass1, IInterface2
    {
        public override int BaseClassValue1 { get; set; }

        public int InterfaceValue { get; set; }
    }
}