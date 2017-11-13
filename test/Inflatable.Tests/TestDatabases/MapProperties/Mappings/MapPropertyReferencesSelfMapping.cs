using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties.Mappings
{
    public class MapPropertyReferencesSelfMapping : MappingBaseClass<MapPropertyReferencesSelf, TestDatabaseMapping>
    {
        public MapPropertyReferencesSelfMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Map(x => x.MappedClass);
        }
    }
}