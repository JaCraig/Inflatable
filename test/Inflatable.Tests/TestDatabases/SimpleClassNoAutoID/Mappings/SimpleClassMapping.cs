using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.SimpleClassNoAutoID.Mappings
{
    public class SimpleClassNoIDMapping : MappingBaseClass<SimpleClassNoID, TestDatabaseMapping>
    {
        public SimpleClassNoIDMapping()
        {
            Reference(x => x.Name);
        }
    }
}