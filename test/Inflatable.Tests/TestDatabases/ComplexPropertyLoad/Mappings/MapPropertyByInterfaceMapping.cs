using Inflatable.BaseClasses;
using Inflatable.Tests.MockClasses;

namespace Inflatable.Tests.TestDatabases.ComplexPropertyLoad.Mappings
{
    public class IMappedClassMapping : MappingBaseClass<IMappedClass, MockDatabaseMapping>
    {
        public IMappedClassMapping()
            : base(merge: true)
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }

    public class MappedClass1Mapping : MappingBaseClass<MappedClass1, MockDatabaseMapping>
    {
        public MappedClass1Mapping()
        {
        }
    }

    public class MappedClass2Mapping : MappingBaseClass<MappedClass2, MockDatabaseMapping>
    {
        public MappedClass2Mapping()
        {
        }
    }

    public class MapPropertyByInterfaceMapping : MappingBaseClass<MapPropertyByInterface, MockDatabaseMapping>
    {
        public MapPropertyByInterfaceMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Map(x => x.MappedClass);
        }
    }
}