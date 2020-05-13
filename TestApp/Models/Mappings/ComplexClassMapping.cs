using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    public class ComplexClassMapping : MappingBaseClass<ComplexClass, TestDatabaseMapping>
    {
        public ComplexClassMapping()
        {
            Reference(x => x.Value2);
            Reference(x => x.Value1);
            ManyToOne(x => x.ManyToOneProperty).CascadeChanges();
        }
    }
}