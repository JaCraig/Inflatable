using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass1Mapping : MappingBaseClass<ConcreteClass1, MockDatabaseMapping>
    {
        public ConcreteClass1Mapping()
        {
            Reference(x => x.Value1);
        }
    }
}