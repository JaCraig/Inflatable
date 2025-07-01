using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    /// <summary>
    /// Provides mapping configuration for <see cref="IComplexClass2"/> to the <see
    /// cref="TestDatabaseMapping"/> database.
    /// </summary>
    public class IComplexClass2Mapping : MappingBaseClass<IComplexClass2, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IComplexClass2Mapping"/> class and sets up
        /// the mapping for the <see cref="IComplexClass2.DateValue"/> property.
        /// </summary>
        public IComplexClass2Mapping()
        {
            Reference(x => x.DateValue);
        }
    }
}