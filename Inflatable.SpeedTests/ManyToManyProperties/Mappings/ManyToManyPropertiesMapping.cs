using Inflatable.BaseClasses;
using Inflatable.SpeedTests.ManyToManyProperties.Databases;

namespace Inflatable.SpeedTests.ManyToManyProperties.Mappings
{
    public class ManyToManyPropertiesMapping : MappingBaseClass<ManyToManyProperties, TestDatabaseMapping>
    {
        public ManyToManyPropertiesMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToMany(x => x.ManyToManyClass);
        }
    }
}