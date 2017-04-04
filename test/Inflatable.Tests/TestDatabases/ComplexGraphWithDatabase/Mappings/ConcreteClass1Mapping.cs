using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass1MappingWithDatabase : MappingBaseClass<ConcreteClass1, TestDatabaseMapping>
    {
        public ConcreteClass1MappingWithDatabase()
        {
            Reference(x => x.Value1);
            Reference(x => x.Value1);
            Reference(x => x.Value1);
        }
    }
}