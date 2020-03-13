using FileCurator.Registration;
using Inflatable.Registration;
using Inflatable.Tests.BaseClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using TestFountain.Registration;
using Xunit;

namespace Inflatable.Tests.Fixtures
{
    [CollectionDefinition("DirectoryCollection")]
    public class CanisterCollection : ICollectionFixture<CanisterFixture>
    {
    }

    public class CanisterFixture : IDisposable
    {
        public CanisterFixture()
        {
            SetupIoC();
        }

        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";
        public static IConfigurationRoot Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();

        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();

        public void Dispose()
        {
            try
            {
                Canister.Builder.Bootstrapper?.Dispose();
            }
            catch { }
        }

        private void SetupIoC()
        {
            _ = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                            .AddAssembly(typeof(TestingFixture).Assembly)
                                            .RegisterInflatable()
                                            .RegisterFileCurator()
                                            .RegisterTestFountain()
                                            .Build();
        }
    }
}