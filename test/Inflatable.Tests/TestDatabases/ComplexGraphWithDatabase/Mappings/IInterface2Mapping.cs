using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class IInterface2MappingWithDatabase : MappingBaseClass<IInterface2, TestDatabaseMapping>
    {
        public IInterface2MappingWithDatabase()
        {
            Reference(x => x.InterfaceValue);
        }
    }
}