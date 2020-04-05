using BenchmarkDotNet.Attributes;
using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mirage.Registration;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace InflatableBenchmarks.Benchmarks.Tests
{
    [MemoryDiagnoser, HtmlExporter]
    public class QueryAndCachingSchemeReferencesOnly
    {
        private DbContext<SimpleClass> Original { get; set; }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            try
            {
                var Configuration = Canister.Builder.Bootstrapper.Resolve<IConfiguration>();
                var Batch = Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
                await Batch.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
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
            Console.WriteLine("Setting up session");
            Canister.Builder.Bootstrapper.Resolve<Session>();

            Console.WriteLine("Setting up values");
            var Values = 5000.Times(x => new SimpleClass() { BoolValue = x % 2 == 0 }).ToArray();

            Console.WriteLine("Saving values");
            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();
            Console.WriteLine("Done");
        }
    }
}