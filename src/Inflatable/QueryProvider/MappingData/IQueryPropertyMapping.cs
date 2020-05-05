using Inflatable.ClassMapper.Interfaces;
using System.Collections.Generic;

namespace Inflatable.QueryProvider.MappingData
{
    /// <summary>
    /// Query property mapping information
    /// </summary>
    public interface IQueryPropertyMapping
    {
        /// <summary>
        /// Gets the foreign ids.
        /// </summary>
        /// <value>The foreign ids.</value>
        List<IIDProperty> ForeignIDs { get; }
    }
}