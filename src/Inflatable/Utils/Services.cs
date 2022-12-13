using Microsoft.Extensions.DependencyInjection;
using System;

namespace Inflatable.Utils
{
    /// <summary>
    /// Services
    /// </summary>
    internal static class Services
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        public static IServiceProvider? ServiceProvider
        {
            get
            {
                if (_ServiceProvider is not null)
                    return _ServiceProvider;
                lock (LockObject)
                {
                    _ServiceProvider = (ServiceCollection ?? new ServiceCollection().AddCanisterModules())?.BuildServiceProvider();
                }
                return _ServiceProvider;
            }
        }

        /// <summary>
        /// The service collection
        /// </summary>
        internal static IServiceCollection? ServiceCollection;

        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object LockObject = new();

        /// <summary>
        /// The service provider
        /// </summary>
        private static IServiceProvider? _ServiceProvider;
    }
}