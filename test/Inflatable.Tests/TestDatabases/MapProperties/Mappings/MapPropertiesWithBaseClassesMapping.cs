using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inflatable.Tests.TestDatabases.MapProperties.Mappings
{
    public class MapPropertiesWithBaseClassesMapping : MappingBaseClass<MapPropertiesWithBaseClasses, TestDatabaseMapping>
    {
        public MapPropertiesWithBaseClassesMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Map(x => x.MappedClass).CascadeChanges();
        }
    }

    public class IMapPropertyInterfaceMapping : MappingBaseClass<IMapPropertyInterface, TestDatabaseMapping>
    {
        public IMapPropertyInterfaceMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }

    public class MapPropertyBaseClassMapping : MappingBaseClass<MapPropertyBaseClass, TestDatabaseMapping>
    {
        public MapPropertyBaseClassMapping()
        {
            Reference(x => x.BaseValue1);
        }
    }

    public class MapProperty1Mapping : MappingBaseClass<MapProperty1, TestDatabaseMapping>
    {
        public MapProperty1Mapping()
        {
            Reference(x => x.ChildValue1);
        }
    }

    public class MapProperty2Mapping : MappingBaseClass<MapProperty2, TestDatabaseMapping>
    {
        public MapProperty2Mapping()
        {
            Reference(x => x.ChildValue2);
        }
    }
}
