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

using BigBook;
using BigBook.DataMapper;
using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inflatable.Schema
{
    /// <summary>
    /// Model manager
    /// </summary>
    public class SchemaManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaManager"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="dataModeler">The data modeler.</param>
        /// <param name="sherlock">The sherlock analyzer.</param>
        /// <param name="stringBuilderPool">The string builder pool.</param>
        /// <param name="aopManager">The aop manager.</param>
        /// <param name="dataMapper">The data mapper.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public SchemaManager(MappingManager mappings, IConfiguration config, ILogger logger, DataModeler dataModeler, Sherlock sherlock, ObjectPool<StringBuilder> stringBuilderPool, Aspectus.Aspectus aopManager, Manager dataMapper)
        {
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Mappings = mappings;
            Models = Mappings.Sources.ToList(x => new DataModel(x, config, logger!, dataModeler, sherlock, stringBuilderPool, aopManager, dataMapper));
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public MappingManager Mappings { get; }

        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>The models.</value>
        public IEnumerable<DataModel> Models { get; }
    }
}