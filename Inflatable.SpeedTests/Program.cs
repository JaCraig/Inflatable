using Microsoft.Extensions.DependencyInjection;
using Sundial.Core.Runner;
using System;

namespace Inflatable.SpeedTests
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            new ServiceCollection().AddCanisterModules(x => x.AddAssembly(typeof(Program).Assembly)
                .RegisterSundial()
                .RegisterInflatable());
            var Runner = Canister.Builder.Bootstrapper.Resolve<TimedTaskRunner>();
            Runner.Run();
            Console.ReadKey();
        }
    }
}