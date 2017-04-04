using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ComplexGraph.Mappings
{
    public class ConcreteClass2MappingWithDatabase : MappingBaseClass<ConcreteClass2, TestDatabaseMapping>
    {
        public ConcreteClass2MappingWithDatabase()
        {
            Reference(x => x.InterfaceValue);
            Reference(x => x.BaseClassValue1);
        }
    }
}