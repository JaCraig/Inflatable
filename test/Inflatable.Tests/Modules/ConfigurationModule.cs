using Canister.Interfaces;
using Inflatable.Tests.BaseClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Inflatable.Tests.Modules
{
    public class ConfigurationModule : IModule
    {
        public int Order => 1;

        protected static string ConnectionString => TestConnectionStrings.Default;

        protected static string ConnectionString2 => TestConnectionStrings.Default2;

        protected static string MockDatabaseConnectionString => TestConnectionStrings.MockDatabase;
        protected static string MockDatabaseForMockMappingConnectionString => TestConnectionStrings.MockDatabaseForMockMapping;

        public void Load(IServiceCollection bootstrapper)
        {
            if (bootstrapper == null)
            {
                return;
            }

            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                    { "ConnectionStrings:Default2", ConnectionString2 },
                    { "ConnectionStrings:MockDatabase",MockDatabaseConnectionString },
                    { "ConnectionStrings:MockDatabaseForMockMapping",MockDatabaseForMockMappingConnectionString }
                };
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            bootstrapper.AddSingleton<IConfiguration>(Configuration);
            bootstrapper.AddSingleton(Configuration);
        }
    }
}