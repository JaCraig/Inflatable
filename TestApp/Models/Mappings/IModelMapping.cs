using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    public class IModelMapping : MappingBaseClass<IModel, TestDatabaseMapping>
    {
        public IModelMapping()
            : base(merge: true)
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }
}