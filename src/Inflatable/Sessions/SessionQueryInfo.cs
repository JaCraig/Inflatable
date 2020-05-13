using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Column.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.Sessions
{
    /// <summary>
    /// Session query info
    /// </summary>
    public class SessionQueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionQueryInfo"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="childMappings">The child mappings.</param>
        /// <param name="parentMappings">The parent mappings.</param>
        public SessionQueryInfo(IMappingSource source, IMapping[] childMappings, IMapping[] parentMappings)
        {
            ChildMappings = childMappings;
            ParentMappings = parentMappings;
            AssociatedMapping = System.Array.Find(ParentMappings, x => x.IDProperties.Count > 0);
            Source = source;
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties).ToArray();
            IDColumnInfo = IDProperties.SelectMany(x => x.GetColumnInfo()).ToArray();
        }

        /// <summary>
        /// Gets or sets the associated mapping.
        /// </summary>
        /// <value>The associated mapping.</value>
        public IMapping AssociatedMapping { get; set; }

        /// <summary>
        /// Gets or sets the child mappings.
        /// </summary>
        /// <value>The child mappings.</value>
        public IMapping[] ChildMappings { get; set; }

        /// <summary>
        /// Gets or sets the identifier column information.
        /// </summary>
        /// <value>The identifier column information.</value>
        public IEnumerable<IQueryColumnInfo> IDColumnInfo { get; set; }

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        public IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the parent mappings.
        /// </summary>
        /// <value>The parent mappings.</value>
        public IMapping[] ParentMappings { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public IMappingSource Source { get; set; }
    }
}