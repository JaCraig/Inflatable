using Inflatable.Interfaces;
using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.TestDatabases.Databases
{
    public class TestDatabase2Mapping : IDatabase
    {
        public bool Audit => true;

        public string Name => "Default2";

        public bool Optimize => true;
        public int Order => 1;

        public DbProviderFactory Provider => SqlClientFactory.Instance;
        public bool Readable => true;

        public bool Update => true;

        public bool Writable => true;
    }
}