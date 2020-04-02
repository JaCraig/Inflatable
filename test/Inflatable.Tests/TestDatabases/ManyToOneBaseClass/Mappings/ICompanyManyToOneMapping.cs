using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToOneBaseClass.Interfaces;

namespace Inflatable.Tests.TestDatabases.ManyToOneBaseClass.Mappings
{
    public class ICompanyManyToOneMapping : MappingBaseClass<ICompanyManyToOne, TestDatabaseMapping>
    {
        public ICompanyManyToOneMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }
}