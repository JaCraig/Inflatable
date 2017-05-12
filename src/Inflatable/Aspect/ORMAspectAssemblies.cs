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
                                        .Where(x => !DontLoad.Contains(x.Name)))
            {
                var TempAssembly = MetadataReference.CreateFromFile(DLL.FullName);
                AssembliesUsing.Add(TempAssembly);
            }
            Assemblies = AssembliesUsing;
        }

        /// <summary>
        /// The assemblies to not load
        /// </summary>
        private string[] DontLoad =
                                        {
"sos.dll",
"mscorrc.dll",
"mscorrc.debug.dll",
"mscordbi.dll",
"mscordaccore.dll",
"libuv.dll",
"hostpolicy.dll",
"hostfxr.dll",
"ext-ms-win-ntuser-keyboard-l1-2-1.dll",
"ext-ms-win-advapi32-encryptedfile-l1-1-0.dll",
"dbgshim.dll",
"coreclr.dll",
"clrjit.dll",
"clretwrc.dll",
"clrcompression.dll",
"api-ms-win-service-winsvc-l1-1-0.dll",
"api-ms-win-service-private-l1-1-1.dll",
"api-ms-win-service-private-l1-1-0.dll",
"api-ms-win-service-management-l2-1-0.dll",
"api-ms-win-service-management-l1-1-0.dll",
"api-ms-win-service-core-l1-1-1.dll",
"api-ms-win-service-core-l1-1-0.dll",
"api-ms-win-security-sddl-l1-1-0.dll",
"api-ms-win-security-provider-l1-1-0.dll",
"API-MS-Win-Security-LsaPolicy-L1-1-0.dll",
"api-ms-win-security-lsalookup-l2-1-1.dll",
"api-ms-win-security-lsalookup-l2-1-0.dll",
"api-ms-win-security-cryptoapi-l1-1-0.dll",
"api-ms-win-security-cpwl-l1-1-0.dll",
"api-ms-win-security-base-l1-1-0.dll",
"api-ms-win-ro-typeresolution-l1-1-0.dll",
"API-MS-Win-EventLog-Legacy-L1-1-0.dll",
"API-MS-Win-Eventing-Provider-L1-1-0.dll",
"API-MS-Win-Eventing-Legacy-L1-1-0.dll",
"API-MS-Win-Eventing-Controller-L1-1-0.dll",
"API-MS-Win-Eventing-Consumer-L1-1-0.dll",
"API-MS-Win-Eventing-ClassicProvider-L1-1-0.dll",
"API-MS-Win-devices-config-L1-1-1.dll",
"API-MS-Win-devices-config-L1-1-0.dll",
"api-ms-win-core-xstate-l2-1-0.dll",
"api-ms-win-core-xstate-l1-1-0.dll",
"api-ms-win-core-wow64-l1-1-0.dll",
"api-ms-win-core-winrt-string-l1-1-0.dll",
"api-ms-win-core-winrt-roparameterizediid-l1-1-0.dll",
"api-ms-win-core-winrt-robuffer-l1-1-0.dll",
"api-ms-win-core-winrt-registration-l1-1-0.dll",
"api-ms-win-core-winrt-l1-1-0.dll",
"api-ms-win-core-winrt-error-l1-1-1.dll",
"api-ms-win-core-winrt-error-l1-1-0.dll",
"api-ms-win-core-version-l1-1-0.dll",
"api-ms-win-core-util-l1-1-0.dll",
"api-ms-win-core-url-l1-1-0.dll",
"api-ms-win-core-timezone-l1-1-0.dll",
"api-ms-win-core-threadpool-private-l1-1-0.dll",
"api-ms-win-core-threadpool-legacy-l1-1-0.dll",
"api-ms-win-core-threadpool-l1-2-0.dll",
"api-ms-win-core-sysinfo-l1-2-3.dll",
"api-ms-win-core-sysinfo-l1-2-2.dll",
"api-ms-win-core-sysinfo-l1-2-1.dll",
"api-ms-win-core-sysinfo-l1-2-0.dll",
"api-ms-win-core-sysinfo-l1-1-0.dll",
"api-ms-win-core-synch-l1-2-0.dll",
"api-ms-win-core-synch-l1-1-0.dll",
"api-ms-win-core-stringloader-l1-1-1.dll",
"api-ms-win-core-stringloader-l1-1-0.dll",
"API-MS-Win-Core-StringAnsi-L1-1-0.dll",
"api-ms-win-core-string-obsolete-l1-1-1.dll",
"api-ms-win-core-string-obsolete-l1-1-0.dll",
"API-MS-Win-Core-String-L2-1-0.dll",
"api-ms-win-core-string-l1-1-0.dll",
"api-ms-win-core-shutdown-l1-1-1.dll",
"api-ms-win-core-shutdown-l1-1-0.dll",
"api-ms-win-core-shlwapi-obsolete-l1-1-0.dll",
"api-ms-win-core-shlwapi-legacy-l1-1-0.dll",
"api-ms-win-core-rtlsupport-l1-1-0.dll",
"api-ms-win-core-registry-l2-1-0.dll",
"api-ms-win-core-registry-l1-1-0.dll",
"api-ms-win-core-realtime-l1-1-0.dll",
"api-ms-win-core-psapi-obsolete-l1-1-0.dll",
"api-ms-win-core-psapi-l1-1-0.dll",
"api-ms-win-core-psapi-ansi-l1-1-0.dll",
"api-ms-win-core-profile-l1-1-0.dll",
"API-MS-Win-Core-ProcessTopology-Obsolete-L1-1-0.dll",
"api-ms-win-core-processthreads-l1-1-2.dll",
"api-ms-win-core-processthreads-l1-1-1.dll",
"api-ms-win-core-processthreads-l1-1-0.dll",
"api-ms-win-core-processsecurity-l1-1-0.dll",
"api-ms-win-core-processenvironment-l1-2-0.dll",
"api-ms-win-core-processenvironment-l1-1-0.dll",
"api-ms-win-core-privateprofile-l1-1-1.dll",
"API-MS-Win-Core-PrivateProfile-L1-1-0.dll",
"api-ms-win-core-normalization-l1-1-0.dll",
"api-ms-win-core-namedpipe-l1-2-1.dll",
"api-ms-win-core-namedpipe-l1-1-0.dll",
"api-ms-win-core-memory-l1-1-3.dll",
"api-ms-win-core-memory-l1-1-2.dll",
"api-ms-win-core-memory-l1-1-1.dll",
"api-ms-win-core-memory-l1-1-0.dll",
"api-ms-win-core-localization-obsolete-l1-2-0.dll",
"api-ms-win-core-localization-l2-1-0.dll",
"api-ms-win-core-localization-l1-2-1.dll",
"api-ms-win-core-localization-l1-2-0.dll",
"api-ms-win-core-libraryloader-l1-1-1.dll",
"api-ms-win-core-libraryloader-l1-1-0.dll",
"API-MS-Win-Core-Kernel32-Private-L1-1-2.dll",
"API-MS-Win-Core-Kernel32-Private-L1-1-1.dll",
"API-MS-Win-Core-Kernel32-Private-L1-1-0.dll",
"api-ms-win-core-kernel32-legacy-l1-1-2.dll",
"api-ms-win-core-kernel32-legacy-l1-1-1.dll",
"api-ms-win-core-kernel32-legacy-l1-1-0.dll",
"api-ms-win-core-io-l1-1-1.dll",
"api-ms-win-core-io-l1-1-0.dll",
"api-ms-win-core-interlocked-l1-1-0.dll",
"api-ms-win-core-heap-obsolete-l1-1-0.dll",
"api-ms-win-core-heap-l1-1-0.dll",
"api-ms-win-core-handle-l1-1-0.dll",
"api-ms-win-core-file-l2-1-1.dll",
"api-ms-win-core-file-l2-1-0.dll",
"api-ms-win-core-file-l1-2-1.dll",
"api-ms-win-core-file-l1-2-0.dll",
"api-ms-win-core-file-l1-1-0.dll",
"api-ms-win-core-fibers-l1-1-1.dll",
"api-ms-win-core-fibers-l1-1-0.dll",
"api-ms-win-core-errorhandling-l1-1-1.dll",
"api-ms-win-core-errorhandling-l1-1-0.dll",
"api-ms-win-core-delayload-l1-1-0.dll",
"api-ms-win-core-debug-l1-1-1.dll",
"api-ms-win-core-debug-l1-1-0.dll",
"api-ms-win-core-datetime-l1-1-1.dll",
"api-ms-win-core-datetime-l1-1-0.dll",
"api-ms-win-core-console-l2-1-0.dll",
"api-ms-win-core-console-l1-1-0.dll",
"api-ms-win-core-comm-l1-1-0.dll",
"api-ms-win-core-com-private-l1-1-0.dll",
"api-ms-win-core-com-l1-1-0.dll",
"API-MS-Win-Base-Util-L1-1-0.dll",
"Microsoft.DiaSymReader.Native.amd64.dll"
        };

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        public List<MetadataReference> Assemblies { get; }
    }
}