using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class IInterface2Mapping : MappingBaseClass<IInterface2, MockDatabaseMapping>
    {
        public IInterface2Mapping()
        {
            Reference(x => x.InterfaceValue);
        }
    }
}