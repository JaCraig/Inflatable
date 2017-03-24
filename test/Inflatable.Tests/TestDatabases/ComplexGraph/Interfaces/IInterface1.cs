using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces
{
    public interface IInterface1
    {
        [IntGenerator]
        int ID { get; set; }
    }
}