using Canister.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Inflatable.Tests.Modules
{
    public class ConfigurationModule : IModule
    {
        public int Order => 1;
        protected readonly string ConnectionString = "Data Source=localhost;Initial Catalog=SpeedTestDatabase;Integrated Security=SSPI;Pooling=false";

        public void Load(IServiceCollection bootstrapper)
        {
            if (bootstrapper == null)
            {
                return;
            }

            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                };
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            bootstrapper.AddSingleton<IConfiguration>(Configuration);
            bootstrapper.AddSingleton<IConfigurationRoot>(Configuration);
        }
    }
}