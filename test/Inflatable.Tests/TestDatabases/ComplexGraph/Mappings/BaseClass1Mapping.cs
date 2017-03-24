using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class BaseClass1Mapping : MappingBaseClass<BaseClass1, MockDatabaseMapping>
    {
        public BaseClass1Mapping()
        {
            Reference(x => x.BaseClassValue1);
        }
    }
}