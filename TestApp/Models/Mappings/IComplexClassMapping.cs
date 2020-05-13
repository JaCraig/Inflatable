using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    public class IComplexClassMapping : MappingBaseClass<ICompexClass, TestDatabaseMapping>
    {
        public IComplexClassMapping()
            : base()
        {
            Reference(x => x.Value3);
        }
    }
}