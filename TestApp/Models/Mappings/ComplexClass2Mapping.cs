using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    public class ComplexClass2Mapping : MappingBaseClass<ComplexClass2, TestDatabaseMapping>
    {
        public ComplexClass2Mapping()
            : base()
        {
            Reference(x => x.StringVal);
        }
    }
}