using Canister.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace InflatableBenchmarks.Benchmarks.Modules
{
    /// <summary>
    /// Module responsible for configuring application settings and registering them with the
    /// dependency injection container.
    /// </summary>
    public class ConfigurationModule : IModule
    {
        /// <summary>
        /// Gets the order in which the module should be loaded.
        /// </summary>
        public int Order => 1;

        /// <summary>
        /// Gets the default connection string for the application.
        /// </summary>
        protected static string ConnectionString => "Data Source=localhost;Initial Catalog=InflatableTestDatabase;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";

        /// <summary>
        /// Loads the configuration settings into the provided service collection.
        /// </summary>
        /// <param name="bootstrapper">The service collection to which configuration will be added.</param>
        public void Load(IServiceCollection bootstrapper)
        {
            if (bootstrapper is null)
                return;
            var Dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                };
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(Dict)
                             .Build();
            bootstrapper.AddSingleton<IConfiguration>(Configuration);
            bootstrapper.AddSingleton(Configuration);
        }
    }
}