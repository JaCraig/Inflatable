using Inflatable.Registration;
using Microsoft.Extensions.DependencyInjection;
using Sundial.Core.Registration;
using Sundial.Core.Runner;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inflatable.SpeedTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                .AddAssembly(typeof(Program).GetTypeInfo().Assembly)
                .RegisterSundial()
                .RegisterInflatable()
                .Build();
            var Runner = Canister.Builder.Bootstrapper.Resolve<TimedTaskRunner>();
            Runner.Run();
            Console.ReadKey();
        }
    }
}