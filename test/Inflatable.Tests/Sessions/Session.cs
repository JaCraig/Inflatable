using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionTests : TestingFixture
    {
        public SessionTests()
        {
            InternalMappingManager = new MappingManager(new[] {
                new AllReferencesAndIDMappingWithDatabase()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, Logger);

            var TempQueryProvider = new SQLServerQueryProvider(Configuration);
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, Logger);
        }

        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public void AllNoParametersAndDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager);
            var Results = TestObject.All<AllReferencesAndIDMappingWithDatabase>();
            Assert.Empty(Results);
        }

        [Fact]
        public void AllNoParametersAndNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager);
            var Results = TestObject.All<AllReferencesAndIDMappingWithDatabase>();
            Assert.Empty(Results);
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager);
            Assert.NotNull(TestObject);
        }
    }
}