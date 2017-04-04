using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings
{
    public class SimpleClassDataSource1MappingWithDatabase : MappingBaseClass<SimpleClass, TestDatabaseMapping>
    {
        public SimpleClassDataSource1MappingWithDatabase()
        {
            ID(x => x.ID);
            Reference(x => x.DataSource1Value);
        }
    }
}