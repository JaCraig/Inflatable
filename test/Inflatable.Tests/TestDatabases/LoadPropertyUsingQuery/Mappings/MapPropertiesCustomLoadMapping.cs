using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using System.Data;

namespace Inflatable.Tests.TestDatabases.LoadPropertyUsingQuery.Mappings
{
    public class MapPropertiesCustomLoadMapping : MappingBaseClass<MapPropertiesCustomLoad, TestDatabaseMapping>
    {
        public MapPropertiesCustomLoadMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Map(x => x.MappedClass).CascadeChanges().LoadUsing("SELECT TOP 1 AllReferencesAndID_.ID_,CharValue_ FROM AllReferencesAndID_ INNER JOIN [TestDatabase].[dbo].[MapPropertiesCustomLoad_] ON AllReferencesAndID_MappedClass_ID_=AllReferencesAndID_.ID_ WHERE MapPropertiesCustomLoad_.ID_=@ID", CommandType.Text);
        }
    }
}