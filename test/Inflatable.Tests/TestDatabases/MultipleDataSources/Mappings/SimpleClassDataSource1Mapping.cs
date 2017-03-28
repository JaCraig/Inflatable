using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings
{
    public class SimpleClassDataSource1Mapping : MappingBaseClass<SimpleClass, SecondMockDatabaseMapping>
    {
        public SimpleClassDataSource1Mapping()
        {
            ID(x => x.ID);
            Reference(x => x.DataSource1Value);
        }
    }
}