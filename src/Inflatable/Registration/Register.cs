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

using BigBook.Registration;
using Canister.Interfaces;
using Data.Modeler.Registration;
using Inflatable;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Registration extension methods
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Registers the library with the bootstrapper.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The bootstrapper</returns>
        public static ICanisterConfiguration? RegisterInflatable(this ICanisterConfiguration? bootstrapper)
        {
            return bootstrapper?.AddAssembly(typeof(Registration).Assembly)
                               ?.RegisterSQLHelper()
                               ?.RegisterDataModeler()
                               ?.RegisterBigBookOfDataTypes()
                               ?.RegisterHolmes()
                               ?.RegisterInMemoryHoard();
        }

        /// <summary>
        /// Registers the Inflatable library with the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to register the library with.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection? RegisterInflatable(this IServiceCollection services)
        {
            if (services.Exists<MappingManager>())
                return services;

            Services.ServiceCollection = services;
            return services?.AddAllTransient<IMapping>()
                ?.AddAllTransient<IDatabase>()
                ?.AddAllTransient<Inflatable.QueryProvider.Interfaces.IQueryProvider>()
                ?.AddSingleton<MappingManager>()
                ?.AddSingleton<SchemaManager>()
                ?.AddSingleton<QueryProviderManager>()
                ?.AddTransient<Session>()
                ?.AddTransient<ISession, Session>()
                ?.AddAllSingleton<IStartMethodHelper>()
                ?.AddAllSingleton<IInterfaceImplementationHelper>()
                ?.AddAllSingleton<IEndMethodHelper>()
                ?.AddTransient(typeof(QueryTranslator<>))
                ?.AddTransient<DbContext>()
                ?.RegisterSQLHelper()
                ?.RegisterDataModeler()
                ?.RegisterBigBookOfDataTypes()
                ?.RegisterHolmes()
                ?.AddInMemoryHoard();
        }
    }
}