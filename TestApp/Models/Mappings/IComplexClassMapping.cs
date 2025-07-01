using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    /// <summary>
    /// Represents the mapping configuration for the <see cref="ICompexClass"/> entity to the <see
    /// cref="TestDatabaseMapping"/> database context.
    /// </summary>
    public class IComplexClassMapping : MappingBaseClass<ICompexClass, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IComplexClassMapping"/> class. Configures
        /// the mapping for the <see cref="ICompexClass.Value3"/> property as a reference.
        /// </summary>
        public IComplexClassMapping()
        {
            Reference(x => x.Value3);
        }
    }
}