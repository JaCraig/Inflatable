using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class IInterface1MappingWithDatabase : MappingBaseClass<IInterface1, TestDatabaseMapping>
    {
        public IInterface1MappingWithDatabase()
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }
}