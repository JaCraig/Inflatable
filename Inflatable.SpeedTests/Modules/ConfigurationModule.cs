using Canister.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Inflatable.Tests.Modules
{
    public class ConfigurationModule : IModule
    {
        protected readonly string ConnectionString = "Data Source=localhost;Initial Catalog=SpeedTestDatabase;Integrated Security=SSPI;Pooling=false";
        public int Order => 1;

        public void Load(IBootstrapper bootstrapper)
        {
            if (bootstrapper == null)
            {
                return;
            }

            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                };
            var Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            bootstrapper.Register<IConfiguration>(Configuration, ServiceLifetime.Singleton);
            bootstrapper.Register<IConfigurationRoot>(Configuration, ServiceLifetime.Singleton);
        }
    }
}