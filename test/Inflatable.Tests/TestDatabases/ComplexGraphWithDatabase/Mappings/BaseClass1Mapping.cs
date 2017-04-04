using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class BaseClass1MappingWithDatabase : MappingBaseClass<BaseClass1, TestDatabaseMapping>
    {
        public BaseClass1MappingWithDatabase()
        {
            Reference(x => x.BaseClassValue1);
        }
    }
}