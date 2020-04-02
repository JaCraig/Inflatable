using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneBaseClass.Mappings
{
    public class CompanyManyToOneMapping : MappingBaseClass<CompanyManyToOne, TestDatabaseMapping>
    {
        public CompanyManyToOneMapping()
        {
            ManyToOne(x => x.IndustryCode);
        }
    }
}