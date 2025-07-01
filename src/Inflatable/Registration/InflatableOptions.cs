using Microsoft.Extensions.Options;
using System;

namespace Inflatable.Registration
{
    /// <summary>
    /// Inflatable options
    /// </summary>
    /// <seealso cref="IOptions{InflatableOptions}"/>
    public class InflatableOptions : IOptions<InflatableOptions>
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static InflatableOptions Default => new()
        {
            ScanFrequency = TimeSpan.FromMinutes(1),
            MaxCacheSize = 1024,
            AbsoluteExpirationQueriesKeptInCache = TimeSpan.FromHours(1),
            SlidingExpirationQueriesKeptInCache = TimeSpan.FromMinutes(1)
        };

        /// <summary>
        /// Gets or sets the absolute expiration queries kept in cache.
        /// </summary>
        /// <value>The absolute expiration queries kept in cache.</value>
        public TimeSpan AbsoluteExpirationQueriesKeptInCache { get; set; }

        /// <summary>
        /// Gets or sets the maximum size.
        /// </summary>
        /// <value>The maximum size.</value>
        public long? MaxCacheSize { get; set; }

        /// <summary>
        /// Gets or sets the scan frequency.
        /// </summary>
        /// <value>The scan frequency.</value>
        public TimeSpan ScanFrequency { get; set; }

        /// <summary>
        /// Gets or sets the sliding expiration queries kept in cache.
        /// </summary>
        /// <value>The sliding expiration queries kept in cache.</value>
        public TimeSpan SlidingExpirationQueriesKeptInCache { get; set; }

        /// <summary>
        /// Gets the default configured Options instance.
        /// </summary>
        public InflatableOptions Value => this;
    }
}