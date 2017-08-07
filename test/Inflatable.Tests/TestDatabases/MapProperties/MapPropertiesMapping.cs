using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MapPropertiesMapping : MappingBaseClass<MapProperties, TestDatabaseMapping>
    {
        public MapPropertiesMapping()
        {
            ID(x => x.ID);
            Reference(x => x.BoolValue);
            Map(x => x.MappedClass);
        }
    }
}