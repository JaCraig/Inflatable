using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass3Mapping : MappingBaseClass<ConcreteClass3, MockDatabaseMapping>
    {
        public ConcreteClass3Mapping()
        {
            Reference(x => x.MyUniqueProperty);
        }
    }
}