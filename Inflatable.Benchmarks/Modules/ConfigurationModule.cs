using Canister.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace InflatableBenchmarks.Benchmarks.Modules
{
    public class ConfigurationModule : IModule
    {
        public int Order => 1;

        protected string ConnectionString => "Data Source=localhost;Initial Catalog=SpeedTestDatabase;Integrated Security=SSPI;Pooling=false";

        protected string ConnectionStringAlt => "Data Source=localhost;Initial Catalog=SpeedTestDatabaseAlt;Integrated Security=SSPI;Pooling=false";

        public void Load(IServiceCollection bootstrapper)
        {
            if (bootstrapper is null)
                return;
            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                    { "ConnectionStrings:DefaultAlt", ConnectionStringAlt },
                };
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            bootstrapper.AddSingleton<IConfiguration>(Configuration);
            bootstrapper.AddSingleton<IConfigurationRoot>(Configuration);
        }
    }
}