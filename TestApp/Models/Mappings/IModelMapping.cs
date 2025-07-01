using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;

namespace TestApp.Models.Mappings
{
    /// <summary>
    /// Provides the mapping configuration for the <see cref="IModel"/> entity to the <see
    /// cref="TestDatabaseMapping"/> database.
    /// </summary>
    public class IModelMapping : MappingBaseClass<IModel, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IModelMapping"/> class. Configures the ID
        /// property as an auto-incremented primary key.
        /// </summary>
        public IModelMapping()
            : base(merge: true)
        {
            ID(x => x.ID).IsAutoIncremented();
        }
    }
}