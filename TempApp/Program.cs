using BigBook;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using InflatableBenchmarks.Benchmarks.Models;
using Microsoft.Extensions.DependencyInjection;
using Mirage;
using Mirage.Registration;
using System.Collections.Generic;
using System.Linq;

namespace TempApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                .AddAssembly(typeof(Program).Assembly)
                .AddAssembly(typeof(InflatableBenchmarks.QueryProvider.Queries).Assembly)
                .RegisterInflatable()
                .RegisterMirage()
                .Build();

            Canister.Builder.Bootstrapper.Resolve<Inflatable.Sessions.Session>();
            Canister.Builder.Bootstrapper.Resolve<InflatableBenchmarks.Sessions.Session>();

            Random random = Canister.Builder.Bootstrapper.Resolve<Random>();
            var Values = 5000.Times(x => random.Next<SimpleClass>()).ToArray();
            var ValuesAlt = 5000.Times(x => random.Next<SimpleClassAlt>()).ToArray();
        }
    }
}