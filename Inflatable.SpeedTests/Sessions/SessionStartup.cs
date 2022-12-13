using Inflatable.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Sundial.Core.Attributes;
using Sundial.Core.Interfaces;
using System;

namespace Inflatable.SpeedTests.Sessions
{
    [Series("Startup", 1, "Console")]
    public class SessionStartup : ITimedTask
    {
        public SessionStartup(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public bool Baseline => true;

        public string Name => "Session Startup";

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
        }

        public void Run() => _ = ServiceProvider.GetService<ISession>();
    }
}