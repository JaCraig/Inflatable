using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using System;
using System.Linq;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Canister.Builder.CreateContainer(null)
                .AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                .Build();
            Console.WriteLine("Setting up session");
            Canister.Builder.Bootstrapper.Resolve<Session>();

            Console.WriteLine("Setting up values");
            var Values = 50.Times(x => new SimpleClass() { BoolValue = x % 2 == 0, StringValue1 = "A", StringValue2 = "ASDFGHKL" }).ToArray();

            Console.WriteLine("Saving values");
            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();

            Console.WriteLine("Querying values");

            var Results = DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();
            Console.WriteLine("Done");
        }
    }
}