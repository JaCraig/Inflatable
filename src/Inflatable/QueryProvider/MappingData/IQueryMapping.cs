using BigBook.DataMapper.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using System.Collections.Generic;

namespace Inflatable.QueryProvider.MappingData
{
    /// <summary>
    /// Query mapping interface
    /// </summary>
    public interface IQueryMapping
    {
        /// <summary>
        /// Gets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        List<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets the parent mappings.
        /// </summary>
        /// <value>The parent mappings.</value>
        List<IMapping> ParentMappings { get; }

        /// <summary>
        /// Gets the primary parent mapping (with primary ID).
        /// </summary>
        /// <value>The primary parent mapping (with primary ID).</value>
        IMapping PrimaryParentMapping { get; }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <value>The schema.</value>
        string Schema { get; }
    }
}