using Inflatable.BaseClasses;
using Inflatable.SpeedTests.ManyToManyProperties.Databases;

namespace Inflatable.SpeedTests.ManyToManyProperties.Mappings
{
    public class IManyToManyPropertyInterfaceManyToMany : MappingBaseClass<IManyToManyPropertyInterface, TestDatabaseMapping>
    {
        public IManyToManyPropertyInterfaceManyToMany()
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }

    public class ManyToManyPropertiesWithBaseClassesManyToMany : MappingBaseClass<ManyToManyPropertiesWithBaseClasses, TestDatabaseMapping>
    {
        public ManyToManyPropertiesWithBaseClassesManyToMany()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            ManyToMany(x => x.ManyToManyClass).CascadeChanges();
        }
    }

    public class ManyToManyProperty1ManyToMany : MappingBaseClass<ManyToManyProperty1, TestDatabaseMapping>
    {
        public ManyToManyProperty1ManyToMany()
        {
            Reference(x => x.ChildValue1);
        }
    }

    public class ManyToManyProperty2ManyToMany : MappingBaseClass<ManyToManyProperty2, TestDatabaseMapping>
    {
        public ManyToManyProperty2ManyToMany()
        {
            Reference(x => x.ChildValue2);
        }
    }

    public class ManyToManyPropertyBaseClassManyToMany : MappingBaseClass<ManyToManyPropertyBaseClass, TestDatabaseMapping>
    {
        public ManyToManyPropertyBaseClassManyToMany()
        {
            Reference(x => x.BaseValue1);
        }
    }
}