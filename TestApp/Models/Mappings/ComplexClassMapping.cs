using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    /// <summary>
    /// Represents the mapping configuration for the <see cref="ComplexClass"/> entity within the
    /// <see cref="TestDatabaseMapping"/> database context.
    /// </summary>
    public class ComplexClassMapping : MappingBaseClass<ComplexClass, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexClassMapping"/> class. Configures
        /// the entity relationships and reference properties.
        /// </summary>
        public ComplexClassMapping()
        {
            // Maps the Value2 property as a reference.
            Reference(x => x.Value2);

            // Maps the Value1 property as a reference.
            Reference(x => x.Value1);

            // Configures the ManyToOneProperty as a many-to-one relationship with cascading changes.
            ManyToOne(x => x.ManyToOneProperty).CascadeChanges();
        }
    }
}