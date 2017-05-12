using Inflatable.Aspect;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Inflatable.Tests.Aspect
{
    public class AspectAssemblies : ORMAspectAssembliesBase
    {
        public AspectAssemblies()
        {
            Assemblies.Add(MetadataReference.CreateFromFile(typeof(AspectAssemblies).GetTypeInfo().Assembly.Location));
        }
    }
}