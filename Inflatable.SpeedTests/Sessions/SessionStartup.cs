using Inflatable.Sessions;
using Sundial.Core.Attributes;
using Sundial.Core.Interfaces;

namespace Inflatable.SpeedTests.Sessions
{
    [Series("Startup", 1, "Console")]
    public class SessionStartup : ITimedTask
    {
        public bool Baseline => true;

        public string Name => "Session Startup";

        public void Dispose()
        {
        }

        public void Run()
        {
            var Session = Canister.Builder.Bootstrapper.Resolve<Session>();
        }
    }
}