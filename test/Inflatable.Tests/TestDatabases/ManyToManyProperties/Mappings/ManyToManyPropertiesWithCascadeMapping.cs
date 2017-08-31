using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
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