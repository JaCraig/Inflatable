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
        private const string StringVal1Const = "A";
        private const string StringVal2Const = "ASDFGHKL";

        private static void Main()
        {
            Canister.Builder.CreateContainer(null)
                .AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                .Build();
            Console.WriteLine("Setting up session");
            Canister.Builder.Bootstrapper.Resolve<Session>();

            Console.WriteLine("Setting up values");
            var Values = 200.Times(x => new SimpleClass() { BoolValue = x % 2 == 0, StringValue1 = StringVal1Const, StringValue2 = StringVal2Const }).ToArray();

            Console.WriteLine("Saving values");
            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();

            Console.WriteLine("Querying values");

            var Results = DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();
            Console.WriteLine("Done");
        }
    }
}