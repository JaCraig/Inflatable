using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class IInterface1Mapping : MappingBaseClass<IInterface1, MockDatabaseMapping>
    {
        public IInterface1Mapping()
        {
            ID(x => x.ID);
        }
    }
}