using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    /// <summary>
    /// Represents the mapping configuration for the <see cref="ComplexClass2"/> entity to the <see
    /// cref="TestDatabaseMapping"/> database.
    /// </summary>
    public class ComplexClass2Mapping : MappingBaseClass<ComplexClass2, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexClass2Mapping"/> class. Configures
        /// the mapping for the <see cref="ComplexClass2.StringVal"/> property as a reference.
        /// </summary>
        public ComplexClass2Mapping()
        {
            Reference(x => x.StringVal);
        }
    }
}