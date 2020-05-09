using BigBook;
using System;

namespace Inflatable.QueryProvider
{
    /// <summary>
    /// Cached result
    /// </summary>
    public class CachedResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResults"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <exception cref="ArgumentNullException">query</exception>
        public CachedResult(Dynamo value, Type objectType)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            ObjectType = objectType;
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public Dynamo Value { get; }
    }
}