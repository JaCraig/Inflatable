using Inflatable.Interfaces;
using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.TestDatabases.Databases
{
    public class TestDatabaseMapping : IDatabase
    {
        public bool Audit => true;

        public string Name => "Default";

        public int Order => 1;

        public bool Readable => true;

        public bool Update => true;

        public bool Writable => true;

        public bool Optimize => true;

        public DbProviderFactory Provider => SqlClientFactory.Instance;
    }
}