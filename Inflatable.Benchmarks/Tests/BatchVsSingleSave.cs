﻿using BenchmarkDotNet.Attributes;
using BigBook;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using InflatableBenchmarks.Benchmarks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Inflatable.Benchmarks.Tests
{
    [MemoryDiagnoser, HtmlExporter]
    public class BatchVsSingleSave
    {
        [Params(10, 50, 100, 1000)]
        public int Count { get; set; }

        private static IServiceProvider ServiceProvider;

        [Benchmark]
        public async Task BatchSaveAsync()
        {
            var Context = new DbContext();
            for (var x = 0; x < Count; ++x)
            {
                Context.Save(new SimpleClass() { BoolValue = x % 2 == 0 });
            }

            await Context.ExecuteAsync().ConfigureAwait(false);
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            try
            {
                IConfiguration? Configuration = ServiceProvider.GetService<IConfiguration>();
                SQLHelper? Batch = ServiceProvider.GetService<SQLHelper>();
                if (Batch is null || Configuration is null)
                    return;
                await Batch.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE SpeedTestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE SpeedTestDatabase SET ONLINE\r\nDROP DATABASE SpeedTestDatabase")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
        }

        [GlobalSetup]
        public void Setup()
        {
            ServiceProvider = new ServiceCollection().AddCanisterModules(x => x.AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                ?.RegisterMirage()).BuildServiceProvider();
            Console.WriteLine("Setting up session");
            ServiceProvider.GetService<Session>();
        }

        [Benchmark]
        public async Task SingleSaveAsync()
        {
            var Tasks = new List<Task>();
            for (var x = 0; x < Count; ++x)
            {
                Tasks.Add(new DbContext().Save(new SimpleClass() { BoolValue = x % 2 == 0 }).ExecuteAsync());
            }
            await Task.WhenAll(Tasks).ConfigureAwait(false);
        }

        [Benchmark(Baseline = true)]
        public void SingleSaveSync()
        {
            for (var x = 0; x < Count; ++x)
            {
                AsyncHelper.RunSync(() => new DbContext().Save(new SimpleClass() { BoolValue = x % 2 == 0 }).ExecuteAsync());
            }
        }
    }
}