using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass3MappingWithDatabase : MappingBaseClass<ConcreteClass3, TestDatabaseMapping>
    {
        public ConcreteClass3MappingWithDatabase()
        {
            Reference(x => x.MyUniqueProperty);
        }
    }
}