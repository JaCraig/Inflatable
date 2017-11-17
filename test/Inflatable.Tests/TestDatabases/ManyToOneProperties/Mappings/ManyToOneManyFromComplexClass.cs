using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings
{
    public class IManyToOneManyMapping : MappingBaseClass<IManyToOneMany, TestDatabaseMapping>
    {
        public IManyToOneManyMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
        }
    }

    public class ManyToOneManyFromComplexClassMapping : MappingBaseClass<ManyToOneManyFromComplexClass, TestDatabaseMapping>
    {
        public ManyToOneManyFromComplexClassMapping()
        {
            ManyToOne(x => x.ManyToOneClass);
        }
    }
}