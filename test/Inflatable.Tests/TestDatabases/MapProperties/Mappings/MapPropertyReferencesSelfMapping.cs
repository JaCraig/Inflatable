using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties.Mappings
{
    public class MapPropertyReferencesSelfMapping : MappingBaseClass<MapPropertyReferencesSelf, TestDatabaseMapping>
    {
        public MapPropertyReferencesSelfMapping()
        {
            Map(x => x.MappedClass);
        }
    }
}