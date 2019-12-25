using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MappedPropertiesWithCascadeMapping : MappingBaseClass<MapPropertiesWithCascade, TestDatabaseMapping>
    {
        public MappedPropertiesWithCascadeMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Map(x => x.MappedClass).CascadeChanges();
        }
    }
}
