using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    public class IComplexClass2Mapping : MappingBaseClass<IComplexClass2, TestDatabaseMapping>
    {
        public IComplexClass2Mapping()
            : base()
        {
            Reference(x => x.DateValue);
        }
    }
}