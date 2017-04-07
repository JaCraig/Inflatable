using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMappingForMockMapping : Interfaces.IDatabase
    {
        public bool Audit => false;

        public string Name => "MockDatabase";

        public bool Optimize => true;
        public int Order => 1;

        public DbProviderFactory Provider => SqlClientFactory.Instance;
        public bool Readable => true;

        public bool Update => false;

        public bool Writable => true;
    }
}