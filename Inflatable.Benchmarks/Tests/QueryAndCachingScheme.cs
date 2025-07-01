using BenchmarkDotNet.Attributes;
using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Sessions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLHelperDB;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InflatableBenchmarks.Benchmarks.Tests
{
    [MemoryDiagnoser, HtmlExporter]
    public class QueryAndCachingSchemeReferencesOnly
    {
        private IServiceProvider? _ServiceProvider;

        [GlobalCleanup]
        public async Task Cleanup()
        {
            try
            {
                IConfiguration? Configuration = _ServiceProvider?.GetService<IConfiguration>();
                SQLHelper? Batch = _ServiceProvider?.GetService<SQLHelper>();
                if (Batch is null || Configuration is null)
                    return;
                await Batch.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE SpeedTestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE SpeedTestDatabase SET ONLINE\r\nDROP DATABASE SpeedTestDatabase")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
        }

        [Benchmark]
        public void NoQuery() => _ = 2500.Times(x => new SimpleClass() { BoolValue = x % 2 == 0 }).ToArray();

        [Benchmark(Baseline = true)]
        public void OriginalQuery() => _ = DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();

        [GlobalSetup]
        public void Setup()
        {
            _ServiceProvider = new ServiceCollection().AddCanisterModules(x => x.AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                ?.RegisterMirage()).BuildServiceProvider();
            Console.WriteLine("Setting up session");
            _ServiceProvider.GetService<Session>();

            Console.WriteLine("Setting up values");
            SimpleClass[] Values = [.. 5000.Times(x => new SimpleClass() { BoolValue = x % 2 == 0 })];

            Console.WriteLine("Saving values");
            AsyncHelper.RunSync(() => new DbContext().Save(Values).ExecuteAsync());

            Console.WriteLine(DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList().Count + " values returned each operation.");

            Console.WriteLine("Done");
        }
    }
}