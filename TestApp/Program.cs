using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                .AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
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