using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
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