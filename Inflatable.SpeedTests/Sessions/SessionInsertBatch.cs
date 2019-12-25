using Inflatable.Sessions;
using Sundial.Core.Attributes;
using Sundial.Core.Interfaces;

namespace Inflatable.SpeedTests.Sessions
{
    [Series("InsertBatch", 10, "HTML")]
    public class SessionInsertBatch : ITimedTask
    {
        public SessionInsertBatch()
        {
            TempSession = Canister.Builder.Bootstrapper.Resolve<Session>();
        }

        private readonly Session TempSession;
        public bool Baseline => false;

        public string Name => "Session Insert Batch";

        public void Dispose()
        {
        }

        public void Run()
        {
            for (var x = 0; x < 5; ++x)
            {
                var TempItem = new ManyToManyProperties.ManyToManyPropertiesWithCascade();
                for (var y = 0; y < 5; ++y)
                {
                    TempItem.ManyToManyClass.Add(new ManyToManyProperties.AllReferencesAndID());
                }
                TempSession.Save(TempItem);
            }
            TempSession.Execute();
        }
    }
}