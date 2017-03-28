using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings
{
    public class SimpleClassDataSource2Mapping : MappingBaseClass<SimpleClass, MockDatabaseMapping>
    {
        public SimpleClassDataSource2Mapping()
        {
            ID(x => x.ID);
            Reference(x => x.DataSource2Value);
        }
    }
}