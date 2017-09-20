using Inflatable.Aspect;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Inflatable.SpeedTests.Aspect
{
    public class AspectAssemblies : ORMAspectAssembliesBase
    {
        public AspectAssemblies()
            : base(
                 new MetadataReference[] {
                    MetadataReference.CreateFromFile(typeof(AspectAssemblies).GetTypeInfo().Assembly.Location)
                 }
            )
        {
        }
    }
}