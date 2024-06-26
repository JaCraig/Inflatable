﻿/*
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

using Canister.Interfaces;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Inflatable.Modules
{
    /// <summary>
    /// Inflatable module
    /// </summary>
    /// <seealso cref="IModule"/>
    public class InflatableModule : IModule
    {
        /// <summary>
        /// Order to run this in
        /// </summary>
        public int Order => 40;

        /// <summary>
        /// Loads the module using the bootstrapper
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IServiceCollection bootstrapper)
        {
            Services.ServiceCollection = bootstrapper;
            bootstrapper?.AddAllTransient<IMapping>()
                ?.AddAllTransient<IDatabase>()
                ?.AddAllTransient<QueryProvider.Interfaces.IQueryProvider>()
                ?.AddSingleton<MappingManager>()
                ?.AddSingleton<SchemaManager>()
                ?.AddSingleton<QueryProviderManager>()
                ?.AddTransient<Session>()
                ?.AddTransient<ISession, Session>()
                ?.AddAllSingleton<IStartMethodHelper>()
                ?.AddAllSingleton<IInterfaceImplementationHelper>()
                ?.AddAllSingleton<IEndMethodHelper>()
                ?.AddTransient(typeof(QueryTranslator<>))
                ?.AddTransient<DbContext>();
        }
    }
}