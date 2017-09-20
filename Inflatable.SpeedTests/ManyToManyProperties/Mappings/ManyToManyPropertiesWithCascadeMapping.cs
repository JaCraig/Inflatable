using Inflatable.BaseClasses;
using Inflatable.SpeedTests.ManyToManyProperties.Databases;

namespace Inflatable.SpeedTests.ManyToManyProperties.Mappings
{
    public class ManyToManyPropertiesWithCascadeMapping : MappingBaseClass<ManyToManyPropertiesWithCascade, TestDatabaseMapping>
    {
        public ManyToManyPropertiesWithCascadeMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToMany(x => x.ManyToManyClass).CascadeChanges();
        }
    }
}