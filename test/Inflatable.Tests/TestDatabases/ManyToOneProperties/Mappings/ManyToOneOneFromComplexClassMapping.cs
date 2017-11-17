using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings
{
    public class IManyToOneOneMapping : MappingBaseClass<IManyToOneOne, TestDatabaseMapping>
    {
        public IManyToOneOneMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
        }
    }

    public class ManyToOneOneFromComplexClassMapping : MappingBaseClass<ManyToOneOneFromComplexClass, TestDatabaseMapping>
    {
        public ManyToOneOneFromComplexClassMapping()
        {
            ManyToOne(x => x.ManyToOneClass);
        }
    }
}