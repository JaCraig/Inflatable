using BenchmarkDotNet.Attributes;
using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mirage;
using Mirage.Registration;
using SQLHelperDB;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace InflatableBenchmarks.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class QueryAndCachingSchemeReferencesOnly
    {
        private DbContext<SimpleClass> Original { get; set; }

        [GlobalCleanup]
        public void Cleanup()
        {
            try
            {
                var Configuration = Canister.Builder.Bootstrapper.Resolve<IConfiguration>();
                new SQLHelper(Configuration, SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .CreateBatch()
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalar<int>();
            }
            catch { }
        }

        [Benchmark(Baseline = true)]
        public void OriginalQuery()
        {
            DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();
        }

        [GlobalSetup]
        public void Setup()
        {
            Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                .AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                .RegisterMirage()
                .Build();

            Canister.Builder.Bootstrapper.Resolve<ISession>();

            Random random = Canister.Builder.Bootstrapper.Resolve<Random>();
            var Values = 5000.Times(x => random.Next<SimpleClass>()).ToArray();

            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}