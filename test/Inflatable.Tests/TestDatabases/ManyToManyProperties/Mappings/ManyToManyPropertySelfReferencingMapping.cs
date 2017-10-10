using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties.Mappings
{
    public class ManyToManyPropertySelfReferencingMapping : MappingBaseClass<ManyToManyPropertySelfReferencing, TestDatabaseMapping>
    {
        public ManyToManyPropertySelfReferencingMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToMany(x => x.Children).SetTableName("Parent_Child").CascadeChanges();
        }
    }
}