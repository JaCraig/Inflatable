using Inflatable.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;

namespace Inflatable.Tests.TestDatabases.ManyToOneBaseClass.Mappings
{
    public class IndustryCodeManyToOneMapping : MappingBaseClass<IndustryCodeManyToOne, TestDatabaseMapping>
    {
        public IndustryCodeManyToOneMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            ManyToOne(x => x.Companies);
        }
    }
}