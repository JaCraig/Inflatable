using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces
{
    public interface IInterface2
    {
        [IntGenerator]
        int InterfaceValue { get; set; }
    }
}