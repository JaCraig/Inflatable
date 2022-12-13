using Microsoft.Extensions.DependencyInjection;
using Sundial.Core.Runner;
using System;

namespace Inflatable.SpeedTests
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ServiceProvider Services = new ServiceCollection().AddCanisterModules(x => x.AddAssembly(typeof(Program).Assembly)
                .RegisterSundial()
                .RegisterInflatable()).BuildServiceProvider();
            TimedTaskRunner Runner = Services.GetService<TimedTaskRunner>();
            Runner.Run();
            Console.ReadKey();
        }
    }
}