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
                    .AddQuery(CommandType.Text, "ALTER DATABASE SpeedTestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE SpeedTestDatabase SET ONLINE\r\nDROP DATABASE SpeedTestDatabase")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
        }

        [Benchmark]
        public void NoQuery()
        {
            _ = 2500.Times(x => new SimpleClass() { BoolValue = x % 2 == 0 }).ToArray();
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

            Console.WriteLine(DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList().Count + " values returned each operation.");

            Console.WriteLine("Done");
        }
    }
}