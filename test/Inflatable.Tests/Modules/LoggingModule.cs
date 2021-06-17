﻿using Canister.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Inflatable.Tests.Modules
{
    /// <summary>
    /// Logging module
    /// </summary>
    /// <seealso cref="IModule"/>
    public class LoggingModule : IModule
    {
        /// <summary>
        /// Order to run this in
        /// </summary>
        public int Order => 1;

        /// <summary>
        /// Loads the module using the bootstrapper
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IBootstrapper bootstrapper)
        {
            if (bootstrapper == null)
            {
                return;
            }

            Log.Logger = new LoggerConfiguration()
                                            .WriteTo
                                            .File("./Log.txt")
                                            .MinimumLevel
                                            .Debug()
                                            .CreateLogger();
            bootstrapper.Register<ILogger>(Log.Logger,
                                        ServiceLifetime.Singleton);
        }
    }
}