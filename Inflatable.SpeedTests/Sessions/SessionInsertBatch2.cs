using BigBook;
using Inflatable.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Sundial.Core.Attributes;
using Sundial.Core.Interfaces;
using System;

namespace Inflatable.SpeedTests.Sessions
{
    [Series("InsertBatch", 10, "HTML")]
    public class SessionInsertBatchAsync : ITimedTask
    {
        public SessionInsertBatchAsync(IServiceProvider serviceProvider)
        {
            TempSession = serviceProvider.GetService<ISession>();
        }

        public bool Baseline => true;
        public string Name => "Session Insert Batch Async";
        private readonly ISession TempSession;

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
            AsyncHelper.RunSync(() => TempSession.ExecuteAsync());
        }
    }
}