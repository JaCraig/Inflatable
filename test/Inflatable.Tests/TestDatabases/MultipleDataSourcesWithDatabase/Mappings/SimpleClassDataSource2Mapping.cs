using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings
{
    public class SimpleClassDataSource2MappingWithDatabase : MappingBaseClass<SimpleClass, TestDatabase2Mapping>
    {
        public SimpleClassDataSource2MappingWithDatabase()
        {
            ID(x => x.ID);
            Reference(x => x.DataSource2Value);
        }
    }
}