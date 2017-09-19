using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings
{
    public class ManyToOneManyCascadePropertiesMapping : MappingBaseClass<ManyToOneManyCascadeProperties, TestDatabaseMapping>
    {
        public ManyToOneManyCascadePropertiesMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToOne(x => x.ManyToOneClass).CascadeChanges();
        }
    }
}