using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties.Mappings
{
    public class IMapPropertiesInterfaceMapping : MappingBaseClass<IMapPropertiesInterface, TestDatabaseMapping>
    {
        public IMapPropertiesInterfaceMapping()
        {
            ID(x => x.ID);
            Reference(x => x.BoolValue);
        }
    }

    public class MapPropertiesFromComplexClassMapping : MappingBaseClass<MapPropertiesFromComplexClass, TestDatabaseMapping>
    {
        public MapPropertiesFromComplexClassMapping()
        {
            Map(x => x.MappedClass).CascadeChanges();
        }
    }
}