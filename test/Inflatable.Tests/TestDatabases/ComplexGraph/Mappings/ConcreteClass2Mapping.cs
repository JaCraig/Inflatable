using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass2Mapping : MappingBaseClass<ConcreteClass2, MockDatabaseMapping>
    {
        public ConcreteClass2Mapping()
        {
            Reference(x => x.InterfaceValue);
            Reference(x => x.BaseClassValue1);
        }
    }
}