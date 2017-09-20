/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using FileCurator;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Inflatable.Aspect
{
    /// <summary>
    /// Holds the ORM aspect's assemblies that it requires.
    /// </summary>
    public abstract class ORMAspectAssembliesBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMAspectAssembliesBase"/> class.
        /// </summary>
        protected ORMAspectAssembliesBase()
            : this(new FileInfo(typeof(object).GetTypeInfo().Assembly.Location).Directory.FullName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMAspectAssembliesBase"/> class.
        /// </summary>
        /// <param name="directory">The directory to search for assemblies.</param>
        protected ORMAspectAssembliesBase(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));

            var AssembliesUsing = new List<MetadataReference>();
            foreach (var DLL in new DirectoryInfo(directory)
                                        .EnumerateFiles("*.dll")
                                        .Where(x => Load.Contains(x.Name)))
            {
                var TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                AssembliesUsing.Add(TempAssembly);
            }
            Assemblies.AddRange(AssembliesUsing);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMAspectAssembliesBase"/> class.
        /// </summary>
        /// <param name="references">The references.</param>
        protected ORMAspectAssembliesBase(MetadataReference[] references)
            : this(new FileInfo(typeof(object).GetTypeInfo().Assembly.Location).Directory.FullName)
        {
            Assemblies.AddRange(references);
        }

        /// <summary>
        /// The assemblies to not load
        /// </summary>
        private string[] Load =
            {
            "mscorlib.dll",
"mscorlib.ni.dll",
"System.Collections.Concurrent.dll",
"System.Collections.dll",
"System.Collections.Immutable.dll",
"System.Runtime.dll",
"System.Threading.Tasks.dll"
        };

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        public List<MetadataReference> Assemblies { get; } = new List<MetadataReference>();
    }
}