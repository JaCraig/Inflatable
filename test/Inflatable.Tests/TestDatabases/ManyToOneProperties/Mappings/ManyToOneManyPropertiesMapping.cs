using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings
{
    public class ManyToOneManyPropertiesMapping : MappingBaseClass<ManyToOneManyProperties, TestDatabaseMapping>
    {
        public ManyToOneManyPropertiesMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToOne(x => x.ManyToOneClass);
        }
    }
}