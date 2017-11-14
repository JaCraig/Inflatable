using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties.Mappings
{
    public class IMapPropertiesInterfaceWithMapMapping : MappingBaseClass<IMapPropertiesInterfaceWithMap, TestDatabaseMapping>
    {
        public IMapPropertiesInterfaceWithMapMapping()
        {
            ID(x => x.ID);
            Map(x => x.MappedClass);
        }
    }

    public class MapPropertiesWithMapOnInterfaceMapping : MappingBaseClass<MapPropertiesWithMapOnInterface, TestDatabaseMapping>
    {
        public MapPropertiesWithMapOnInterfaceMapping()
        {
            Reference(x => x.BoolValue);
        }
    }
}